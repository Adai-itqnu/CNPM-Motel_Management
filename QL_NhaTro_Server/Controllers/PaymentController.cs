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

                // Create booking record (Pending status)
                var booking = new Booking
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = dto.UserId,
                    RoomId = dto.RoomId,
                    Status = BookingStatus.Pending,
                    DepositAmount = dto.DepositAmount,
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
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
            try
            {
                // Verify VNPAY signature
                if (!VerifyVNPaySignature(callback))
                {
                    return BadRequest(new { message = "Chữ ký không hợp lệ" });
                }

                // Check if payment successful
                if (callback.vnp_ResponseCode != "00")
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thanh toán thất bại",
                        code = callback.vnp_ResponseCode
                    });
                }

                var bookingId = callback.vnp_TxnRef;
                var booking = await _db.Bookings
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return NotFound(new { message = "Không tìm thấy booking" });
                }

                // Update booking status
                booking.Status = BookingStatus.Approved;
                booking.UpdatedAt = DateTime.Now;

                // Update room status
                booking.Room!.Status = RoomStatus.Occupied;
                booking.Room.UpdatedAt = DateTime.Now;

                // AUTO-CREATE CONTRACT
                var contract = new Contract
                {
                    Id = Guid.NewGuid().ToString(),
                    RoomId = booking.RoomId,
                    UserId = booking.UserId,
                    StartDate = booking.StartDate ?? DateTime.Now,
                    EndDate = booking.EndDate ?? DateTime.Now.AddMonths(6),
                    MonthlyPrice = booking.Room!.Price,
                    DepositAmount = booking.DepositAmount,
                    Status = ContractStatus.Active,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _db.Contracts.Add(contract);

                // Create payment record
                var payment = new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = booking.UserId,
                    BillId = null, // Deposit payment, no bill yet
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

                return Ok(new PaymentResultDto
                {
                    Success = true,
                    Message = "Thanh toán thành công! Hợp đồng đã được tạo tự động.",
                    TransactionId = callback.vnp_TransactionNo,
                    ContractId = contract.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi xử lý callback", error = ex.Message });
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
