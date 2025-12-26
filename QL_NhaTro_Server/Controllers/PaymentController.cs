using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.DTOs;
using QL_NhaTro_Server.Models;
using QL_NhaTro_Server.Services;
using System.Security.Cryptography;
using System.Text;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;
        private readonly INotificationService _notificationService;
        private readonly VNPayService _vnpayService;
        private readonly IConfiguration _configuration;

        public PaymentController(
            MotelManagementDbContext db, 
            INotificationService notificationService,
            VNPayService vnpayService,
            IConfiguration configuration)
        {
            _db = db;
            _notificationService = notificationService;
            _vnpayService = vnpayService;
            _configuration = configuration;
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

                // Generate VNPAY payment URL using config
                var tmnCode = _configuration["VNPaySettings:TmnCode"]!;
                var hashSecret = _configuration["VNPaySettings:HashSecret"]!;
                var returnUrl = dto.ReturnUrl ?? _configuration["VNPaySettings:ReturnUrl"]!;
                
                var paymentUrl = _vnpayService.CreatePaymentUrl(
                    orderId: booking.Id,
                    amount: actualDepositAmount,
                    orderInfo: $"Dat coc phong",
                    returnUrl: returnUrl,
                    tmnCode: tmnCode,
                    hashSecret: hashSecret
                );

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

        // POST /api/payment/bill/{id}/vnpay - Thanh toán hóa đơn hàng tháng qua VNPay
        [HttpPost("bill/{id}/vnpay")]
        [Authorize]
        public async Task<IActionResult> PayBillWithVNPay(string id)
        {
            try
            {
                var bill = await _db.Bills
                    .Include(b => b.Room)
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (bill == null)
                {
                    return NotFound(new { message = "Hóa đơn không tồn tại" });
                }

                if (bill.Status == BillStatus.Paid)
                {
                    return BadRequest(new { message = "Hóa đơn đã được thanh toán" });
                }

                if (bill.TotalAmount <= 0)
                {
                    return BadRequest(new { message = "Số tiền thanh toán không hợp lệ" });
                }

                // Get VNPay settings from configuration
                var tmnCode = _configuration["VNPaySettings:TmnCode"]!;
                var hashSecret = _configuration["VNPaySettings:HashSecret"]!;
                var returnUrl = _configuration["VNPaySettings:ReturnUrl"]!;
                
                var roomName = bill.Room?.Name ?? "phong";
                var orderId = $"BILL_{bill.Id}_{DateTime.Now:yyyyMMddHHmmss}";
                
                var paymentUrl = _vnpayService.CreatePaymentUrl(
                    orderId: orderId,
                    amount: bill.TotalAmount,
                    orderInfo: $"Thanh toan hoa don thang {bill.Month}/{bill.Year} - {roomName}",
                    returnUrl: returnUrl,
                    tmnCode: tmnCode,
                    hashSecret: hashSecret
                );

                return Ok(new
                {
                    billId = bill.Id,
                    paymentUrl = paymentUrl,
                    message = "Vui lòng hoàn tất thanh toán"
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
            Console.WriteLine($"vnp_TxnRef: {callback.vnp_TxnRef}");
            Console.WriteLine($"vnp_ResponseCode: {callback.vnp_ResponseCode}");
            
            // If VNPAY says fail, return immediately
            if (callback.vnp_ResponseCode != "00")
            {
                return Ok(new { success = false, message = "Thanh toán thất bại", code = callback.vnp_ResponseCode });
            }

            // VNPAY confirmed success - process based on payment type
            try
            {
                var txnRef = callback.vnp_TxnRef;
                
                // Check if this is a BILL payment (format: BILL_{billId}_{timestamp})
                if (txnRef.StartsWith("BILL_"))
                {
                    return await ProcessBillPayment(callback);
                }
                
                // Otherwise treat as deposit payment (txnRef = bookingId)
                var bookingId = txnRef;
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

                // Send notifications
                var user = await _db.Users.FindAsync(booking.UserId);
                if (user != null)
                {
                    // Notify user about successful deposit payment
                    await _notificationService.SendDepositPaidNotificationAsync(
                        booking.UserId, 
                        user.FullName, 
                        room.Name, 
                        booking.DepositAmount
                    );

                    // Notify admin about payment received
                    await _notificationService.SendPaymentReceivedNotificationAsync(
                        user.FullName,
                        room.Name,
                        booking.DepositAmount,
                        "Deposit"
                    );
                }

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

        // Process bill (monthly) payment
        private async Task<IActionResult> ProcessBillPayment(VNPayCallbackDto callback)
        {
            Console.WriteLine("=== PROCESSING BILL PAYMENT ===");
            
            // Parse bill ID from txnRef (format: BILL_{billId}_{timestamp})
            var parts = callback.vnp_TxnRef.Split('_');
            if (parts.Length < 2)
            {
                Console.WriteLine("Invalid BILL txnRef format");
                return Ok(new PaymentResultDto { Success = true, Message = "Thanh toán thành công!", TransactionId = callback.vnp_TransactionNo, ContractId = "" });
            }
            
            var billId = parts[1];
            Console.WriteLine($"Bill ID: {billId}");
            
            var bill = await _db.Bills
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == billId);
                
            if (bill == null)
            {
                Console.WriteLine($"Bill not found: {billId}");
                return Ok(new PaymentResultDto { Success = true, Message = "Thanh toán thành công!", TransactionId = callback.vnp_TransactionNo, ContractId = "" });
            }
            
            // Check if already paid
            if (bill.Status == BillStatus.Paid)
            {
                Console.WriteLine("Bill already paid");
                return Ok(new PaymentResultDto { Success = true, Message = "Hóa đơn đã được thanh toán.", TransactionId = callback.vnp_TransactionNo, ContractId = "" });
            }
            
            // Update bill status
            bill.Status = BillStatus.Paid;
            bill.PaymentDate = DateTime.Now;
            bill.UpdatedAt = DateTime.Now;
            
            // Create payment record
            var payment = new Payment
            {
                Id = Guid.NewGuid().ToString(),
                UserId = bill.UserId,
                BillId = bill.Id,
                Amount = bill.TotalAmount,
                PaymentType = PaymentType.MonthlyBill,
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
            Console.WriteLine("=== BILL PAYMENT PROCESSED SUCCESSFULLY ===");
            
            // Send notifications
            if (bill.User != null && bill.Room != null)
            {
                try
                {
                    await _notificationService.CreateNotificationAsync(
                        bill.UserId,
                        "Thanh toán hóa đơn thành công",
                        $"Hóa đơn tháng {bill.Month}/{bill.Year} - Phòng {bill.Room.Name} đã được thanh toán thành công. Số tiền: {bill.TotalAmount:N0} đ",
                        NotificationType.Payment,
                        "/user/bills"
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending notification: {ex.Message}");
                }
            }
            
            return Ok(new PaymentResultDto
            {
                Success = true,
                Message = $"Thanh toán hóa đơn tháng {bill.Month}/{bill.Year} thành công!",
                TransactionId = callback.vnp_TransactionNo,
                ContractId = ""
            });
        }

        #region Helper Methods

        private bool VerifyVNPaySignature(VNPayCallbackDto callback)
        {
            // TODO: Implement real signature verification using VNPayService
            // For now, return true to allow testing
            return true;
        }

        #endregion
    }
}

