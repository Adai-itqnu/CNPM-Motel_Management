using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.DTOs;
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
                .Where(b => b.UserId == userId && b.IsSent) // Chỉ lấy hóa đơn đã gửi
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .Select(b => new
                {
                    b.Id,
                    RoomName = b.Room.Name,
                    b.Month,
                    b.Year,
                    b.DaysInMonth,
                    b.DaysRented,
                    b.ElectricityOldIndex,
                    b.ElectricityNewIndex,
                    b.ElectricityTotal,
                    b.WaterOldIndex,
                    b.WaterNewIndex,
                    b.WaterTotal,
                    b.RoomPrice,
                    b.OtherFees,
                    b.TotalAmount,
                    Status = b.Status.ToString(),
                    b.IsSent,
                    b.DueDate,
                    b.PaymentDate,
                    b.Notes,
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

        // POST /api/user/upload-avatar - Upload avatar for current user
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Vui lòng chọn file ảnh" });
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new { message = "Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif)" });
            }

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "File ảnh không được vượt quá 5MB" });
            }

            try
            {
                // Create uploads directory if not exists
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user avatar URL
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Delete old avatar file if exists
                if (!string.IsNullOrEmpty(user.AvatarUrl))
                {
                    var oldFileName = Path.GetFileName(user.AvatarUrl);
                    var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                user.AvatarUrl = $"/uploads/avatars/{fileName}";
                user.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                return Ok(new 
                { 
                    message = "Cập nhật avatar thành công",
                    avatarUrl = user.AvatarUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi upload avatar: " + ex.Message });
            }
        }

        // PUT /api/user/profile - Update current user's profile
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }

            // Update fields
            if (!string.IsNullOrEmpty(dto.FullName))
                user.FullName = dto.FullName;
            
            if (!string.IsNullOrEmpty(dto.Email))
                user.Email = dto.Email;
            
            if (!string.IsNullOrEmpty(dto.Phone))
                user.Phone = dto.Phone;
            
            if (!string.IsNullOrEmpty(dto.IdCard))
                user.IdCard = dto.IdCard;
            
            if (dto.Address != null)
                user.Address = dto.Address;

            user.UpdatedAt = DateTime.Now;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin thành công" });
        }
    }
}
