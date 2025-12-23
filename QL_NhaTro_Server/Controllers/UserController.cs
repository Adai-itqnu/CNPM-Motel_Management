using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.DTOs;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public class UserController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;

        public UserController(MotelManagementDbContext db)
        {
            _db = db;
        }

        // GET: api/users - Lấy danh sách người thuê
        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _db.Users.AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => 
                    u.Username.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.FullName.Contains(search) ||
                    (u.Phone != null && u.Phone.Contains(search)));
            }

            // Lọc theo role
            if (!string.IsNullOrEmpty(role) && Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                query = query.Where(u => u.Role == userRole);
            }

            // Lọc theo trạng thái active
            if (isActive.HasValue)
            {
                query = query.Where(u => u.IsActive == isActive.Value);
            }

            var total = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    IdCard = u.IdCard,
                    Role = u.Role.ToString(),
                    AvatarUrl = u.AvatarUrl,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                data = users,
                total,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        // GET: api/users/{id} - Lấy thông tin chi tiết người thuê
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var response = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                IdCard = user.IdCard,
                Role = user.Role.ToString(),
                AvatarUrl = user.AvatarUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }

        // GET: api/users/{id}/rental-history - Lịch sử thuê phòng
        [HttpGet("{id}/rental-history")]
        public async Task<IActionResult> GetRentalHistory(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            // Lấy danh sách hợp đồng
            var contracts = await _db.Contracts
                .Include(c => c.Room)
                .Where(c => c.TenantId == id)
                .OrderByDescending(c => c.StartDate)
                .Select(c => new
                {
                    c.Id,
                    RoomName = c.Room.Name,
                    c.StartDate,
                    c.EndDate,
                    c.MonthlyPrice,
                    c.DepositAmount,
                    Status = c.Status.ToString(),
                    c.CreatedAt
                })
                .ToListAsync();

            // Lấy danh sách booking
            var bookings = await _db.Bookings
                .Include(b => b.Room)
                .Where(b => b.UserId == id)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.Id,
                    RoomName = b.Room.Name,
                    b.CheckInDate,
                    b.DepositAmount,
                    Status = b.Status.ToString(),
                    b.CreatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                contracts,
                bookings
            });
        }

        // GET: api/users/{id}/bills - Hóa đơn của người thuê
        [HttpGet("{id}/bills")]
        public async Task<IActionResult> GetUserBills(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            var bills = await _db.Bills
                .Include(b => b.Room)
                .Where(b => b.TenantId == id)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .Select(b => new
                {
                    b.Id,
                    RoomName = b.Room.Name,
                    b.Month,
                    b.Year,
                    b.TotalAmount,
                    Status = b.Status.ToString(),
                    b.PaymentDate,
                    b.DueDate,
                    b.CreatedAt
                })
                .ToListAsync();

            return Ok(bills);
        }

        // PUT: api/users/{id} - Cập nhật thông tin người dùng
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserDto dto)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            if (!string.IsNullOrEmpty(dto.FullName)) user.FullName = dto.FullName;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.IdCard != null) user.IdCard = dto.IdCard;
            if (dto.AvatarUrl != null) user.AvatarUrl = dto.AvatarUrl;

            user.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Cập nhật thông tin thành công" });
        }

        // PATCH: api/users/{id}/toggle-status - Khóa/mở tài khoản
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy người dùng" });

            // Không cho phép khóa tài khoản admin
            if (user.Role == UserRole.Admin)
                return BadRequest(new { message = "Không thể khóa tài khoản Admin" });

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.Now;

            await _db.SaveChangesAsync();

            return Ok(new 
            { 
                message = user.IsActive ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản",
                isActive = user.IsActive
            });
        }
    }
}