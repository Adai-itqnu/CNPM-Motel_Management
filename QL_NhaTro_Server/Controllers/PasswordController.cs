using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QL_NhaTro_Server.DTOs;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PasswordController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;

        public PasswordController(MotelManagementDbContext db)
        {
            _db = db;
        }

        // POST: api/password/change
        [HttpPost("change")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User not authenticated" });

            Console.WriteLine($"[ChangePassword] Request for user ID: {userId}");

            try
            {
                var user = await _db.Users.FindAsync(userId);
                if (user == null)
                {
                    Console.WriteLine($"[ChangePassword] User not found: {userId}");
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
                {
                    Console.WriteLine($"[ChangePassword] Current password incorrect for user: {userId}");
                    return BadRequest(new { message = "Mật khẩu hiện tại không đúng" });
                }

                // Check if new password is same as current
                if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
                {
                    Console.WriteLine($"[ChangePassword] New password same as old for user: {userId}");
                    return BadRequest(new { message = "Mật khẩu mới không được trùng với mật khẩu cũ" });
                }

                // Update password
                Console.WriteLine($"[ChangePassword] Updating password for user: {userId}");
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.UpdatedAt = DateTime.Now;

                await _db.SaveChangesAsync();
                Console.WriteLine($"[ChangePassword] Password updated successfully for user: {userId}");

                return Ok(new { message = "Đổi mật khẩu thành công" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ChangePassword] ERROR: {ex.Message}");
                Console.WriteLine($"[ChangePassword] StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Lỗi: " + ex.Message });
            }
        }
    }
}
