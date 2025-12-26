using Microsoft.EntityFrameworkCore;
using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.Services
{
    public interface INotificationService
    {
        Task<Notification> CreateNotificationAsync(string userId, string title, string content, NotificationType type, string? link = null, string? senderId = null);
        Task SendToAllUsersAsync(string title, string content, NotificationType type, string? link = null, string? senderId = null);
        Task SendToAdminsAsync(string title, string content, NotificationType type, string? link = null);
        
        // Auto notifications
        Task SendWelcomeNotificationAsync(string userId, string userName);
        Task SendDepositPaidNotificationAsync(string userId, string userName, string roomName, decimal amount);
        Task SendNewBillNotificationAsync(string userId, string roomName, int month, int year, decimal amount);
        Task SendBillSentNotificationAsync(string userId, string roomName, int month, int year);
        Task SendOverdueWarningNotificationAsync(string userId, string roomName, int month, int year);
        
        // Notifications for Admin
        Task SendPaymentReceivedNotificationAsync(string tenantName, string roomName, decimal amount, string paymentType);
        Task SendOverdueAlertToAdminAsync(string tenantName, string roomName, int month, int year);
    }

    public class NotificationService : INotificationService
    {
        private readonly MotelManagementDbContext _db;

        public NotificationService(MotelManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Notification> CreateNotificationAsync(string userId, string title, string content, NotificationType type, string? link = null, string? senderId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                Link = link,
                SenderId = senderId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
            return notification;
        }

        public async Task SendToAllUsersAsync(string title, string content, NotificationType type, string? link = null, string? senderId = null)
        {
            var userIds = await _db.Users
                .Where(u => u.IsActive && u.Role == UserRole.User)
                .Select(u => u.Id)
                .ToListAsync();

            var notifications = userIds.Select(userId => new Notification
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                Link = link,
                SenderId = senderId,
                IsRead = false,
                CreatedAt = DateTime.Now
            }).ToList();

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }

        public async Task SendToAdminsAsync(string title, string content, NotificationType type, string? link = null)
        {
            var adminIds = await _db.Users
                .Where(u => u.IsActive && u.Role == UserRole.Admin)
                .Select(u => u.Id)
                .ToListAsync();

            var notifications = adminIds.Select(adminId => new Notification
            {
                UserId = adminId,
                Title = title,
                Content = content,
                Type = type,
                Link = link,
                SenderId = null, // System notification
                IsRead = false,
                CreatedAt = DateTime.Now
            }).ToList();

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }

        // === Auto Notifications for Users ===

        public async Task SendWelcomeNotificationAsync(string userId, string userName)
        {
            await CreateNotificationAsync(
                userId,
                "Chào mừng bạn đến với Nhà Trọ!",
                $"Xin chào {userName}! Cảm ơn bạn đã đăng ký tài khoản. Hãy khám phá các phòng trọ và đặt phòng ngay nhé!",
                NotificationType.System,
                "/user/rooms"
            );
        }

        public async Task SendDepositPaidNotificationAsync(string userId, string userName, string roomName, decimal amount)
        {
            await CreateNotificationAsync(
                userId,
                "Thanh toán cọc thành công!",
                $"Bạn đã thanh toán tiền cọc {amount:N0} VND cho phòng {roomName}. Vui lòng chờ admin xác nhận và hẹn ngày nhận phòng.",
                NotificationType.Payment,
                "/user/bookings"
            );
        }

        public async Task SendNewBillNotificationAsync(string userId, string roomName, int month, int year, decimal amount)
        {
            await CreateNotificationAsync(
                userId,
                $"Hóa đơn tháng {month}/{year}",
                $"Hóa đơn phòng {roomName} tháng {month}/{year} số tiền {amount:N0} VND đã được tạo. Vui lòng chờ admin cập nhật chỉ số điện nước.",
                NotificationType.Payment
            );
        }

        public async Task SendBillSentNotificationAsync(string userId, string roomName, int month, int year)
        {
            await CreateNotificationAsync(
                userId,
                $"Hóa đơn tháng {month}/{year} cần thanh toán",
                $"Hóa đơn phòng {roomName} tháng {month}/{year} đã sẵn sàng. Vui lòng thanh toán trước ngày đến hạn.",
                NotificationType.Payment,
                "/user/bills"
            );
        }

        public async Task SendOverdueWarningNotificationAsync(string userId, string roomName, int month, int year)
        {
            await CreateNotificationAsync(
                userId,
                "⚠️ Cảnh báo: Hóa đơn quá hạn!",
                $"Hóa đơn phòng {roomName} tháng {month}/{year} đã quá hạn thanh toán 1 tuần. Vui lòng thanh toán ngay để tránh bị phạt.",
                NotificationType.Warning,
                "/user/bills"
            );
        }

        // === Notifications for Admin ===

        public async Task SendPaymentReceivedNotificationAsync(string tenantName, string roomName, decimal amount, string paymentType)
        {
            var title = paymentType == "Deposit" 
                ? "Khách đã thanh toán tiền cọc" 
                : "Khách đã thanh toán hóa đơn";
            
            var content = $"{tenantName} đã thanh toán {amount:N0} VND cho phòng {roomName}.";

            await SendToAdminsAsync(title, content, NotificationType.Payment, "/admin/payments");
        }

        public async Task SendOverdueAlertToAdminAsync(string tenantName, string roomName, int month, int year)
        {
            await SendToAdminsAsync(
                "⚠️ Khách trễ hạn thanh toán",
                $"{tenantName} (phòng {roomName}) chưa thanh toán hóa đơn tháng {month}/{year} sau 1 tuần.",
                NotificationType.Warning,
                "/admin/bills"
            );
        }
    }
}
