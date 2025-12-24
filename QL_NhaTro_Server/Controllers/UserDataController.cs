using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.Models;
using System.Security.Claims;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    [Authorize]
    public class UserDataController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;

        public UserDataController(MotelManagementDbContext db)
        {
            _db = db;
        }

        // GET /api/user/my-bills - Get current user's bills
        [HttpGet("my-bills")]
        public async Task<IActionResult> GetMyBills()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var bills = await _db.Bills
                .Include(b => b.Room)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .Select(b => new
                {
                    b.Id,
                    RoomName = b.Room.Name,
                    b.Month,
                    b.Year,
                    b.ElectricityTotal,
                    b.WaterTotal,
                    b.RoomPrice,
                    b.OtherFees,
                    b.TotalAmount,
                    Status = b.Status.ToString(),
                    b.DueDate,
                    b.PaymentDate,
                    b.CreatedAt
                })
                .ToListAsync();

            return Ok(bills);
        }

        // GET /api/user/my-payments - Get current user's payment history
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var payments = await _db.Payments
                .Include(p => p.Booking)
                    .ThenInclude(b => b.Room)
                .Include(p => p.Bill)
                    .ThenInclude(b => b.Room)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.PaymentDate)
                .Select(p => new
                {
                    p.Id,
                    // Nếu là Deposit, lấy tiền cọc từ Room; nếu không thì lấy Amount gốc
                    Amount = p.PaymentType == PaymentType.Deposit && p.Booking != null && p.Booking.Room != null
                        ? (p.Booking.Room.DepositAmount > 0 ? p.Booking.Room.DepositAmount : p.Booking.Room.Price)
                        : p.Amount,
                    PaymentType = p.PaymentType.ToString(),
                    p.PaymentMethod,
                    Status = p.Status.ToString(),
                    p.Provider,
                    p.ProviderTxnId,
                    p.PaymentDate,
                    RoomName = p.Booking != null && p.Booking.Room != null 
                        ? p.Booking.Room.Name 
                        : (p.Bill != null && p.Bill.Room != null ? p.Bill.Room.Name : "N/A"),
                    BillMonth = p.Bill != null ? p.Bill.Month : (int?)null,
                    BillYear = p.Bill != null ? p.Bill.Year : (int?)null,
                    p.CreatedAt
                })
                .ToListAsync();

            return Ok(payments);
        }
    }
}
