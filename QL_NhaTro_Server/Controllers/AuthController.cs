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

        public AuthController(MotelManagementDbContext db, JwtService jwt)
        {
            _db = db;
            _jwt = jwt;
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
                PasswordHash = HashPassword(dto.Password), // SHA256 - nhanh!
                FullName = dto.FullName,
                Phone = dto.Phone,
                Role = isFirstUser ? UserRole.Admin : UserRole.Tenant, // User đầu tiên = Admin
                IsActive = true,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            var message = isFirstUser 
                ? "Đăng ký thành công! Bạn là Admin đầu tiên của hệ thống." 
                : "Đăng ký thành công";

            return Ok(new { message, role = user.Role.ToString().ToLower() });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // Tối ưu: Tìm riêng username hoặc email (nhanh hơn OR)
            var user = await _db.Users
                .AsNoTracking() // Không track changes - nhanh hơn
                .FirstOrDefaultAsync(x => x.Username == dto.UsernameOrEmail);
            
            if (user == null)
            {
                user = await _db.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Email == dto.UsernameOrEmail);
            }

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
                    role = user.Role.ToString().ToLower() // lowercase để match frontend
                }
            });
        }
    }
}
