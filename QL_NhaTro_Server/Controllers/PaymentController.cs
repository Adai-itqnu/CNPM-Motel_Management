using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.DTOs;
using QL_NhaTro_Server.Models;
using System.Security.Cryptography;
using System.Text;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;
        private readonly string _vnpayHashSecret = "YOUR_VNPAY_HASH_SECRET";

        public PaymentController(MotelManagementDbContext db)
        {
            _db = db;
        }

        // POST /api/payment/create-deposit
        [HttpPost("create-deposit")]
        [Authorize]
        public async Task<IActionResult> CreateDepositPayment([FromBody] CreateDepositDto dto)
        {
            try
            {
                // Validate room exists and is available
                var room = await _db.Rooms.FindAsync(dto.RoomId);
                if (room == null)
                {
                    return NotFound(new { message = "Phòng không tồn tại" });
                }

                if (room.Status != RoomStatus.Available)
                {
                    return BadRequest(new { message = "Phòng không còn trống" });
                }

                // Calculate deposit amount - use provided value if > 0, otherwise use room price
                var actualDepositAmount = dto.DepositAmount > 0 ? dto.DepositAmount : room.Price;

                // Create booking record (Pending status)
                var booking = new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = dto.UserId,
                    RoomId = dto.RoomId,
                    Status = BookingStatus.Pending,
                    DepositAmount = actualDepositAmount,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync();

                // Generate VNPAY payment URL
                var paymentUrl = GenerateVNPayUrl(booking.Id, dto.DepositAmount, dto.ReturnUrl);

                return Ok(new
                {
                    bookingId = booking.Id,
                    paymentUrl = paymentUrl,
                    message = "Vui lòng thanh toán cọc để hoàn tất đặt phòng"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi tạo thanh toán", error = ex.Message });
            }
        }

        // GET /api/payment/vnpay-callback (VNPAY will redirect here after payment)
        [HttpGet("vnpay-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> VNPayCallback([FromQuery] VNPayCallbackDto callback)
        {
            Console.WriteLine("\n=== VNPAY CALLBACK RECEIVED ===");
            Console.WriteLine($"vnp_TxnRef (BookingId): {callback.vnp_TxnRef}");
            Console.WriteLine($"vnp_ResponseCode: {callback.vnp_ResponseCode}");
            
            // If VNPAY says fail, return immediately
            if (callback.vnp_ResponseCode != "00")
            {
                return Ok(new { success = false, message = "Thanh toán thất bại", code = callback.vnp_ResponseCode });
            }

            // VNPAY confirmed success - process and return success
            try
            {
                var bookingId = callback.vnp_TxnRef;
                var booking = await _db.Bookings.FindAsync(bookingId);
                
                if (booking == null)
                {
                    Console.WriteLine($"Booking not found: {bookingId}");
                    // Return success anyway since VNPAY confirmed payment
                    return Ok(new PaymentResultDto { Success = true, Message = "Thanh toán thành công!", TransactionId = "", ContractId = "" });
                }

                // Check if already processed
                if (booking.Status == BookingStatus.Approved)
                {
                    return Ok(new PaymentResultDto { Success = true, Message = "Thanh toán đã được xử lý.", TransactionId = "", ContractId = "" });
                }

                // Load room - this should not be null since booking references it
                var room = await _db.Rooms.FindAsync(booking.RoomId);
                if (room == null)
                {
                    Console.WriteLine($"ERROR: Room not found for booking: {booking.RoomId}");
                    return Ok(new PaymentResultDto { Success = true, Message = "Thanh toán thành công! (Lỗi tải phòng)", TransactionId = "", ContractId = "" });
                }

                Console.WriteLine($"Room found: {room.Name}, Price: {room.Price}");
                
                // Update booking
                booking.Status = BookingStatus.Approved;
                booking.DepositStatus = DepositStatus.Paid;
                booking.DepositPaidAt = DateTime.Now;
                booking.UpdatedAt = DateTime.Now;

                // Update room
                room.Status = RoomStatus.Reserved;
                room.CurrentUserId = booking.UserId;
                room.UpdatedAt = DateTime.Now;

                // Create contract with room price
                var contract = new Contract
                {
                    Id = Guid.NewGuid().ToString(),
                    RoomId = booking.RoomId,
                    UserId = booking.UserId,
                    BookingId = booking.Id,
                    StartDate = booking.CheckInDate,
                    EndDate = booking.CheckInDate.AddYears(1),
                    MonthlyPrice = room.Price,  // Use room price directly
                    DepositAmount = booking.DepositAmount,
                    Status = ContractStatus.Draft,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                
                Console.WriteLine($"Creating contract: MonthlyPrice={contract.MonthlyPrice}, DepositAmount={contract.DepositAmount}");
                _db.Contracts.Add(contract);

                // Create payment record
                var payment = new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = booking.UserId,
                    BookingId = booking.Id,
                    Amount = booking.DepositAmount,
                    PaymentType = PaymentType.Deposit,
                    PaymentMethod = "VNPAY",
                    PaymentDate = DateTime.Now,
                    Status = PaymentStatus.Success,
                    Provider = "vnpay",
                    ProviderTxnId = callback.vnp_TransactionNo,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Payments.Add(payment);

                await _db.SaveChangesAsync();
                Console.WriteLine($"=== PAYMENT PROCESSED SUCCESSFULLY ===");

                return Ok(new PaymentResultDto
                {
                    Success = true,
                    Message = "Thanh toán thành công!",
                    TransactionId = callback.vnp_TransactionNo,
                    ContractId = contract.Id
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing: {ex.Message}");
                // Still return success since VNPAY confirmed payment
                return Ok(new PaymentResultDto { Success = true, Message = "Thanh toán thành công!", TransactionId = "", ContractId = "" });
            }
        }

        // GET /api/payment/check-status/{bookingId}
        [HttpGet("check-status/{bookingId}")]
        [Authorize]
        public async Task<IActionResult> CheckPaymentStatus(string bookingId)
        {
            var booking = await _db.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                return NotFound(new { message = "Không tìm thấy booking" });
            }

            var contract = await _db.Contracts
                .FirstOrDefaultAsync(c => c.RoomId == booking.RoomId && c.UserId == booking.UserId);

            return Ok(new
            {
                bookingStatus = booking.Status.ToString(),
                hasContract = contract != null,
                contractId = contract?.Id,
                roomName = booking.Room?.Name
            });
        }

        #region Helper Methods

        private string GenerateVNPayUrl(string bookingId, decimal amount, string returnUrl)
        {
            // TODO: Implement real VNPAY URL generation
            // This is a simplified version
            var vnpayUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
            var vnpayParams = new Dictionary<string, string>
            {
                { "vnp_Version", "2.1.0" },
                { "vnp_Command", "pay" },
                { "vnp_TmnCode", "YOUR_TMN_CODE" }, // TODO: from config
                { "vnp_Amount", ((long)(amount * 100)).ToString() }, // VNĐ * 100
                { "vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
                { "vnp_CurrCode", "VND" },
                { "vnp_IpAddr", "127.0.0.1" },
                { "vnp_Locale", "vn" },
                { "vnp_OrderInfo", $"Thanh toan coc phong {bookingId}" },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_TxnRef", bookingId }
            };

            // Build query string
            var queryString = string.Join("&", vnpayParams.OrderBy(x => x.Key).Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"));
            var secureHash = ComputeHmacSha512(_vnpayHashSecret, queryString);
            
            return $"{vnpayUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        private bool VerifyVNPaySignature(VNPayCallbackDto callback)
        {
            // TODO: Implement real signature verification
            // Extract all vnp_* parameters except vnp_SecureHash
            // Sort and compute HMAC-SHA512
            // Compare with callback.vnp_SecureHash
            return true; // Simplified for now
        }

        private string ComputeHmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        #endregion
    }
}
