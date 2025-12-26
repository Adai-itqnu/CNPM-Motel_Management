using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.Models;
using QL_NhaTro_Server.Services;
using System.Security.Claims;

namespace QL_NhaTro_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly MotelManagementDbContext _db;
        private readonly INotificationService _notificationService;

        public NotificationController(MotelManagementDbContext db, INotificationService notificationService)
        {
            _db = db;
            _notificationService = notificationService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        // GET /api/notification
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var query = _db.Notifications
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.CreatedAt);

                var totalCount = await query.CountAsync();
                var notifications = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new
                    {
                        n.Id,
                        n.Title,
                        n.Content,
                        Type = n.Type.ToString(),
                        n.IsRead,
                        n.Link,
                        n.CreatedAt,
                        SenderName = n.Sender != null ? n.Sender.FullName : null
                    })
                    .ToListAsync();

                return Ok(new
                {
                    notifications,
                    totalCount,
                    page,
                    pageSize,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông báo", error = ex.Message });
            }
        }

        // GET /api/notification/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var count = await _db.Notifications
                    .CountAsync(n => n.UserId == userId && !n.IsRead);

                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đếm thông báo", error = ex.Message });
            }
        }

        // PUT /api/notification/{id}/read
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            try
            {
                var userId = GetUserId();
                var notification = await _db.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (notification == null)
                    return NotFound(new { message = "Không tìm thấy thông báo" });

                notification.IsRead = true;
                await _db.SaveChangesAsync();

                return Ok(new { message = "Đã đánh dấu đã đọc" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông báo", error = ex.Message });
            }
        }

        // PUT /api/notification/read-all
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = GetUserId();
                await _db.Notifications
                    .Where(n => n.UserId == userId && !n.IsRead)
                    .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

                return Ok(new { message = "Đã đánh dấu tất cả đã đọc" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông báo", error = ex.Message });
            }
        }

        // DELETE /api/notification/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(string id)
        {
            try
            {
                var userId = GetUserId();
                var notification = await _db.Notifications
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (notification == null)
                    return NotFound(new { message = "Không tìm thấy thông báo" });

                _db.Notifications.Remove(notification);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Đã xóa thông báo" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi xóa thông báo", error = ex.Message });
            }
        }

        // === Admin Endpoints ===

        // POST /api/notification/admin/send
        [HttpPost("admin/send")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminSendNotification([FromBody] AdminSendNotificationDto dto)
        {
            try
            {
                var senderId = GetUserId();

                if (dto.SendToAll)
                {
                    await _notificationService.SendToAllUsersAsync(
                        dto.Title,
                        dto.Content,
                        NotificationType.Admin,
                        dto.Link,
                        senderId
                    );
                    return Ok(new { message = "Đã gửi thông báo đến tất cả người dùng" });
                }
                else if (!string.IsNullOrEmpty(dto.UserId))
                {
                    var user = await _db.Users.FindAsync(dto.UserId);
                    if (user == null)
                        return NotFound(new { message = "Không tìm thấy người dùng" });

                    await _notificationService.CreateNotificationAsync(
                        dto.UserId,
                        dto.Title,
                        dto.Content,
                        NotificationType.Admin,
                        dto.Link,
                        senderId
                    );
                    return Ok(new { message = $"Đã gửi thông báo đến {user.FullName}" });
                }
                else
                {
                    return BadRequest(new { message = "Vui lòng chọn người nhận hoặc gửi đến tất cả" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi gửi thông báo", error = ex.Message });
            }
        }

        // GET /api/notification/admin/users - Get users for notification dropdown
        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsersForNotification()
        {
            try
            {
                var users = await _db.Users
                    .Where(u => u.IsActive && u.Role == UserRole.User)
                    .Select(u => new
                    {
                        u.Id,
                        u.FullName,
                        u.Email,
                        RoomName = _db.Contracts
                            .Where(c => c.UserId == u.Id && c.Status == ContractStatus.Active)
                            .Select(c => c.Room.Name)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách người dùng", error = ex.Message });
            }
        }

        // GET /api/notification/admin/sent - Get recently sent notifications
        [HttpGet("admin/sent")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRecentSentNotifications()
        {
            try
            {
                var senderId = GetUserId();
                
                var notifications = await _db.Notifications
                    .Where(n => n.SenderId == senderId)
                    .OrderByDescending(n => n.CreatedAt)
                    .Take(20)
                    .Select(n => new
                    {
                        n.Id,
                        n.Title,
                        n.Content,
                        Type = n.Type.ToString(),
                        RecipientName = n.User != null ? n.User.FullName : "Tất cả",
                        n.CreatedAt
                    })
                    .ToListAsync();

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy thông báo đã gửi", error = ex.Message });
            }
        }
    }

    // DTOs
    public class AdminSendNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? UserId { get; set; }  // null nếu gửi tất cả
        public string TargetType { get; set; } = "all"; // "all" hoặc "user"
        public bool SendToAll => TargetType == "all"; // Computed property
        public string? Type { get; set; } // notification type from frontend
        public string? Link { get; set; }
    }
}

