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
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;
        private readonly JwtService _jwt;
        private readonly INotificationService _notificationService;

        public AuthController(MotelManagementDbContext db, JwtService jwt, INotificationService notificationService)
        {
            _db = db;
            _jwt = jwt;
            _notificationService = notificationService;
        }

        // Hash password đơn giản bằng SHA256
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Tối ưu: Check cả username và email trong 1 query
            var existingUser = await _db.Users
                .AsNoTracking()
                .Where(x => x.Username == dto.Username || x.Email == dto.Email)
                .Select(x => new { x.Username, x.Email })
                .FirstOrDefaultAsync();

            if (existingUser != null)
            {
                if (existingUser.Username == dto.Username)
                    return BadRequest("Username đã tồn tại");
                return BadRequest("Email đã tồn tại");
            }

            // Kiểm tra xem có user nào trong hệ thống chưa
            var isFirstUser = !await _db.Users.AnyAsync();

            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password), // SHA256 - cực nhanh!
                FullName = dto.FullName,
                Phone = dto.Phone,
                IdCard = dto.IdCard,
                Address = dto.Address,
                Role = isFirstUser ? UserRole.Admin : UserRole.User, // User đầu tiên = Admin
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Gửi thông báo chào mừng cho user mới (không gửi cho admin đầu tiên)
            // Wrap trong try-catch để không ảnh hưởng đến việc đăng ký
            if (!isFirstUser)
            {
                try
                {
                    await _notificationService.SendWelcomeNotificationAsync(user.Id, user.FullName);
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không throw - đăng ký vẫn thành công
                    Console.WriteLine($"Warning: Failed to send welcome notification: {ex.Message}");
                }
            }

            var message = isFirstUser 
                ? "Đăng ký thành công! Bạn là Admin đầu tiên của hệ thống." 
                : "Đăng ký thành công";

            return Ok(new { message, role = user.Role.ToString().ToLower() });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // OPTIMIZED: Single query với OR (nhanh hơn 2x!)
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Username == dto.UsernameOrEmail || x.Email == dto.UsernameOrEmail);

            if (user == null)
                return Unauthorized("Tài khoản không tồn tại");

            // So sánh hash password
            if (HashPassword(dto.Password) != user.PasswordHash)
                return Unauthorized("Mật khẩu không đúng");

            if (!user.IsActive)
                return Unauthorized("Tài khoản đã bị khóa");
            
            var token = _jwt.GenerateToken(user);

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FullName,
                    user.Phone,
                    user.IdCard,
                    user.Address,
                    user.AvatarUrl,
                    role = user.Role.ToString().ToLower() // lowercase để match frontend
                }
            });
        }
    }
}
