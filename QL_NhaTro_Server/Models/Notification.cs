using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaTro_Server.Models
{
    public enum NotificationType
    {
        System,     // Thông báo hệ thống (đăng ký, chào mừng)
        Payment,    // Thông báo thanh toán (cọc, hóa đơn)
        Warning,    // Cảnh báo (quá hạn thanh toán)
        Admin       // Thông báo từ admin
    }

    [Table("Notifications")]
    public class Notification
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SenderId { get; set; }  // Người gửi (admin) - null nếu hệ thống tự gửi

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "text")]
        public string Content { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; } = NotificationType.System;

        public bool IsRead { get; set; } = false;

        [MaxLength(500)]
        public string? Link { get; set; }  // Link điều hướng khi click vào thông báo

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("SenderId")]
        public virtual User? Sender { get; set; }
    }
}
