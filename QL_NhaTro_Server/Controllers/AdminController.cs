using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.Models;
using QL_NhaTro_Server.Services;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;
        private readonly IBillGenerationService _billGenerationService;
        private readonly INotificationService _notificationService;

        public AdminController(
            MotelManagementDbContext db, 
            IBillGenerationService billGenerationService,
            INotificationService notificationService)
        {
            _db = db;
            _billGenerationService = billGenerationService;
            _notificationService = notificationService;
        }

        // ========== BOOKING MANAGEMENT ==========

        // GET /api/admin/bookings
        [HttpGet("bookings")]
        public async Task<IActionResult> GetAllBookings()
        {
            var bookings = await _db.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    b.RoomId,
                    RoomName = b.Room.Name,
                    b.UserId,
                    UserName = b.User.FullName,
                    UserEmail = b.User.Email,
                    UserPhone = b.User.Phone,
                    b.CheckInDate,
                    // Lấy tiền cọc từ Room, nếu = 0 thì lấy giá phòng
                    DepositAmount = b.Room.DepositAmount > 0 ? b.Room.DepositAmount : b.Room.Price,
                    DepositStatus = b.DepositStatus.ToString(),
                    Status = b.Status.ToString(),
                    b.AdminNote,
                    b.CreatedAt,
                    b.UpdatedAt
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // PUT /api/admin/bookings/{id}/status
        [HttpPut("bookings/{id}/status")]
        public async Task<IActionResult> UpdateBookingStatus(string id, [FromBody] UpdateBookingStatusDto dto)
        {
            var booking = await _db.Bookings
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound(new { message = "Booking không tồn tại" });
            }

            if (Enum.TryParse<BookingStatus>(dto.Status, out var status))
            {
                booking.Status = status;
                booking.AdminNote = dto.AdminNote;
                booking.UpdatedAt = DateTime.Now;

                // If cancelled, release the room
                if (status == BookingStatus.Cancelled && booking.Room != null)
                {
                    if (booking.Room.Status == RoomStatus.Reserved)
                    {
                        booking.Room.Status = RoomStatus.Available;
                        booking.Room.CurrentUserId = null;
                        booking.Room.UpdatedAt = DateTime.Now;
                    }
                }

                await _db.SaveChangesAsync();
                return Ok(new { message = "Cập nhật trạng thái thành công" });
            }

            return BadRequest(new { message = "Trạng thái không hợp lệ" });
        }

        // ========== CONTRACT MANAGEMENT ==========

        // GET /api/admin/contracts
        [HttpGet("contracts")]
        public async Task<IActionResult> GetAllContracts()
        {
            var contracts = await _db.Contracts
                .Include(c => c.Room)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.RoomId,
                    RoomName = c.Room.Name,
                    c.UserId,
                    UserName = c.User.FullName,
                    UserEmail = c.User.Email,
                    UserPhone = c.User.Phone,
                    c.StartDate,
                    c.EndDate,
                    c.MonthlyPrice,
                    // Lấy tiền cọc từ Room, nếu = 0 thì lấy giá phòng
                    DepositAmount = c.Room.DepositAmount > 0 ? c.Room.DepositAmount : c.Room.Price,
                    Status = c.Status.ToString(),
                    c.CreatedAt,
                    c.UpdatedAt
                })
                .ToListAsync();

            return Ok(contracts);
        }

        // POST /api/admin/contracts/{id}/terminate
        [HttpPost("contracts/{id}/terminate")]
        public async Task<IActionResult> TerminateContract(string id, [FromBody] TerminateContractDto dto)
        {
            var contract = await _db.Contracts
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contract == null)
            {
                return NotFound(new { message = "Hợp đồng không tồn tại" });
            }

            if (contract.Status != ContractStatus.Active)
            {
                return BadRequest(new { message = "Chỉ có thể chấm dứt hợp đồng đang hoạt động" });
            }

            contract.Status = ContractStatus.Terminated;
            contract.UpdatedAt = DateTime.Now;

            // Release the room
            if (contract.Room != null)
            {
                contract.Room.Status = RoomStatus.Available;
                contract.Room.CurrentUserId = null;
                contract.Room.CurrentContractId = null;
                contract.Room.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Đã chấm dứt hợp đồng thành công" });
        }

        // POST /api/admin/contracts/{id}/extend
        [HttpPost("contracts/{id}/extend")]
        public async Task<IActionResult> ExtendContract(string id, [FromBody] ExtendContractDto dto)
        {
            var contract = await _db.Contracts.FindAsync(id);

            if (contract == null)
            {
                return NotFound(new { message = "Hợp đồng không tồn tại" });
            }

            if (contract.Status != ContractStatus.Active && contract.Status != ContractStatus.Draft)
            {
                return BadRequest(new { message = "Chỉ có thể gia hạn hợp đồng đang hoạt động hoặc chờ nhận phòng" });
            }

            // Extend the end date
            contract.EndDate = contract.EndDate.Value.AddMonths(dto.ExtendMonths);
            contract.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();
            
            return Ok(new { 
                message = $"Đã gia hạn hợp đồng thêm {dto.ExtendMonths} tháng",
                newEndDate = contract.EndDate.Value.ToString("dd/MM/yyyy")
            });
        }

        // ========== PAYMENT MANAGEMENT ==========

        // GET /api/admin/payments - Get all payments
        [HttpGet("payments")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _db.Payments
                .Include(p => p.User)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .Include(p => p.Bill)
                    .ThenInclude(b => b.Room)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    RoomName = p.Booking != null && p.Booking.Room != null 
                        ? p.Booking.Room.Name 
                        : (p.Bill != null && p.Bill.Room != null ? p.Bill.Room.Name : "N/A"),
                    UserName = p.User.FullName,
                    p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    p.PaymentMethod,
                    Status = p.Status.ToString(),
                    p.ProviderTxnId,
                    p.PaymentDate,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(payments);
        }

        // ========== BILL MANAGEMENT ==========

        // GET /api/admin/bills
        [HttpGet("bills")]
        public async Task<IActionResult> GetAllBills()
        {
            var bills = await _db.Bills
                .Include(b => b.Room)
                .Include(b => b.User)
                .Include(b => b.Contract)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    b.ContractId,
                    b.RoomId,
                    RoomName = b.Room.Name,
                    b.UserId,
                    UserName = b.User.FullName,
                    b.Month,
                    b.Year,
                    b.DaysInMonth,
                    b.DaysRented,
                    b.ElectricityOldIndex,
                    b.ElectricityNewIndex,
                    b.ElectricityPrice,
                    b.ElectricityTotal,
                    b.WaterOldIndex,
                    b.WaterNewIndex,
                    b.WaterPrice,
                    b.WaterTotal,
                    b.RoomPrice,
                    b.OtherFees,
                    b.TotalAmount,
                    b.IsSent,
                    Status = b.Status.ToString(),
                    b.DueDate,
                    b.PaymentDate,
                    b.Notes,
                    b.CreatedAt
                })
                .ToListAsync();

            return Ok(bills);
        }

        // POST /api/admin/bills/generate - Tự động tạo hóa đơn cho tháng
        [HttpPost("bills/generate")]
        public async Task<IActionResult> GenerateBills([FromBody] GenerateBillsDto dto)
        {
            try
            {
                var month = dto.Month ?? DateTime.Now.Month;
                var year = dto.Year ?? DateTime.Now.Year;

                var bills = await _billGenerationService.GenerateMonthlyBillsAsync(month, year);

                return Ok(new
                {
                    message = $"Đã tạo {bills.Count} hóa đơn cho tháng {month}/{year}",
                    count = bills.Count,
                    bills = bills.Select(b => new { b.Id, b.RoomId, b.RoomPrice, b.DaysRented, b.DaysInMonth })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo hóa đơn", error = ex.Message });
            }
        }

        // PUT /api/admin/bills/{id}/update-meters - Admin cập nhật chỉ số điện/nước
        [HttpPut("bills/{id}/update-meters")]
        public async Task<IActionResult> UpdateBillMeters(string id, [FromBody] UpdateBillMetersDto dto)
        {
            var bill = await _db.Bills
                .Include(b => b.Room)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
            {
                return NotFound(new { message = "Hóa đơn không tồn tại" });
            }

            // Update electricity
            bill.ElectricityNewIndex = dto.ElectricityNewIndex;
            var electricityUnits = dto.ElectricityNewIndex - bill.ElectricityOldIndex;
            bill.ElectricityTotal = electricityUnits * bill.ElectricityPrice;

            // Update water
            bill.WaterNewIndex = dto.WaterNewIndex;
            var waterUnits = dto.WaterNewIndex - bill.WaterOldIndex;
            bill.WaterTotal = waterUnits * bill.WaterPrice;

            // Update other fees if provided
            if (dto.OtherFees.HasValue)
            {
                bill.OtherFees = dto.OtherFees.Value;
            }

            // Recalculate total
            bill.TotalAmount = bill.RoomPrice + bill.ElectricityTotal + bill.WaterTotal + bill.OtherFees;

            // Update notes if provided
            if (!string.IsNullOrEmpty(dto.Notes))
            {
                bill.Notes = dto.Notes;
            }

            bill.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã cập nhật chỉ số điện nước",
                bill = new
                {
                    bill.Id,
                    bill.ElectricityOldIndex,
                    bill.ElectricityNewIndex,
                    bill.ElectricityTotal,
                    bill.WaterOldIndex,
                    bill.WaterNewIndex,
                    bill.WaterTotal,
                    bill.RoomPrice,
                    bill.OtherFees,
                    bill.TotalAmount
                }
            });
        }

        // POST /api/admin/bills/{id}/send - Gửi hóa đơn đến người thuê
        [HttpPost("bills/{id}/send")]
        public async Task<IActionResult> SendBillToTenant(string id)
        {
            var bill = await _db.Bills
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bill == null)
            {
                return NotFound(new { message = "Hóa đơn không tồn tại" });
            }

            if (bill.IsSent)
            {
                return BadRequest(new { message = "Hóa đơn đã được gửi trước đó" });
            }

            // Validate bill has meters filled
            if (bill.ElectricityNewIndex == 0 && bill.WaterNewIndex == 0)
            {
                return BadRequest(new { message = "Vui lòng cập nhật chỉ số điện nước trước khi gửi" });
            }

            // Mark as sent
            bill.IsSent = true;
            bill.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            // Send notification to user
            await _notificationService.SendBillSentNotificationAsync(
                bill.UserId,
                bill.Room.Name,
                bill.Month,
                bill.Year
            );

            return Ok(new { message = $"Đã gửi hóa đơn đến {bill.User.FullName}" });
        }

        // GET /api/admin/payments/deposits
        [HttpGet("payments/deposits")]
        public async Task<IActionResult> GetDepositPayments()
        {
            var deposits = await _db.Payments
                .Where(p => p.PaymentType == PaymentType.Deposit)
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.BookingId,
                    RoomName = p.Booking != null ? p.Booking.Room.Name : "N/A",
                    p.UserId,
                    UserName = p.User.FullName,
                    // Lấy tiền cọc từ Room.DepositAmount, nếu = 0 thì lấy Room.Price
                    Amount = p.Booking != null 
                        ? (p.Booking.Room.DepositAmount > 0 ? p.Booking.Room.DepositAmount : p.Booking.Room.Price)
                        : p.Amount,
                    PaymentMethod = p.PaymentMethod,
                    Status = p.Status.ToString(),
                    p.Provider,
                    p.ProviderTxnId,
                    p.PaymentDate,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(deposits);
        }

        // POST /api/admin/bills
        [HttpPost("bills")]
        public async Task<IActionResult> CreateBill([FromBody] CreateBillDto dto)
        {
            // Get contract to validate
            var contract = await _db.Contracts
                .Include(c => c.Room)
                .FirstOrDefaultAsync(c => c.Id == dto.ContractId);

            if (contract == null)
            {
                return NotFound(new { message = "Hợp đồng không tồn tại" });
            }

            // Calculate totals
            var electricityUnits = dto.ElectricityNewIndex - dto.ElectricityOldIndex;
            var electricityTotal = electricityUnits * (contract.Room?.ElectricityPrice ?? 0);
            
            var waterUnits = dto.WaterNewIndex - dto.WaterOldIndex;
            var waterTotal = waterUnits * (contract.Room?.WaterPrice ?? 0);
            
            var totalAmount = electricityTotal + waterTotal + contract.MonthlyPrice + (dto.OtherFees ?? 0);

            var bill = new Bill
            {
                Id = Guid.NewGuid().ToString(),
                ContractId = dto.ContractId,
                RoomId = contract.RoomId,
                UserId = contract.UserId,
                Month = dto.Month,
                Year = dto.Year,
                ElectricityOldIndex = dto.ElectricityOldIndex,
                ElectricityNewIndex = dto.ElectricityNewIndex,
                ElectricityPrice = contract.Room?.ElectricityPrice ?? 0,
                ElectricityTotal = electricityTotal,
                WaterOldIndex = dto.WaterOldIndex,
                WaterNewIndex = dto.WaterNewIndex,
                WaterPrice = contract.Room?.WaterPrice ?? 0,
                WaterTotal = waterTotal,
                RoomPrice = contract.MonthlyPrice,
                OtherFees = dto.OtherFees ?? 0,
                TotalAmount = totalAmount,
                Status = BillStatus.Pending,
                DueDate = dto.DueDate ?? DateTime.Now.AddDays(10),
                Notes = dto.Notes,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Bills.Add(bill);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Tạo hóa đơn thành công", billId = bill.Id });
        }

        // PUT /api/admin/bills/{id}/status
        [HttpPut("bills/{id}/status")]
        public async Task<IActionResult> UpdateBillStatus(string id, [FromBody] UpdateBillStatusDto dto)
        {
            var bill = await _db.Bills.FindAsync(id);

            if (bill == null)
            {
                return NotFound(new { message = "Hóa đơn không tồn tại" });
            }

            if (Enum.TryParse<BillStatus>(dto.Status, out var status))
            {
                bill.Status = status;
                if (status == BillStatus.Paid)
                {
                    bill.PaymentDate = DateTime.Now;
                }
                bill.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync();
                return Ok(new { message = "Cập nhật trạng thái hóa đơn thành công" });
            }

            return BadRequest(new { message = "Trạng thái không hợp lệ" });
        }

        // POST /api/admin/fix-deposit-amounts
        // Fix all rooms with DepositAmount = 0 to use Price instead
        [HttpPost("fix-deposit-amounts")]
        public async Task<IActionResult> FixDepositAmounts()
        {
            var roomsToFix = await _db.Rooms
                .Where(r => r.DepositAmount == 0)
                .ToListAsync();

            if (roomsToFix.Count == 0)
            {
                return Ok(new { message = "Không có phòng nào cần cập nhật", updated = 0 });
            }

            foreach (var room in roomsToFix)
            {
                room.DepositAmount = room.Price;
                room.UpdatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();

            return Ok(new { 
                message = $"Đã cập nhật tiền cọc cho {roomsToFix.Count} phòng",
                updated = roomsToFix.Count,
                rooms = roomsToFix.Select(r => new { r.Id, r.Name, r.DepositAmount }).ToList()
            });
        }
    }

    // DTOs
    public class UpdateBookingStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
    }

    public class TerminateContractDto
    {
        public string? Reason { get; set; }
    }

    public class ExtendContractDto
    {
        public int ExtendMonths { get; set; } = 12; // Default 12 months
    }

    public class CreateBillDto
    {
        public string ContractId { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public int ElectricityOldIndex { get; set; }
        public int ElectricityNewIndex { get; set; }
        public int WaterOldIndex { get; set; }
        public int WaterNewIndex { get; set; }
        public decimal? OtherFees { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }
    }

    public class UpdateBillStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class GenerateBillsDto
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

    public class UpdateBillMetersDto
    {
        public int ElectricityNewIndex { get; set; }
        public int WaterNewIndex { get; set; }
        public decimal? OtherFees { get; set; }
        public string? Notes { get; set; }
    }
}

