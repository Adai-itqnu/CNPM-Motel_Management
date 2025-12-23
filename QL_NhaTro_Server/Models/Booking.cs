using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaTro_Server.Models
{
    public enum DepositStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    public enum BookingStatus
    {
        Pending,
        Approved,
        Cancelled,
        Rejected
    }

    [Table("Bookings")]
    public class Booking
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "date")]
        public DateTime CheckInDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [Column(TypeName = "text")]
        public string? Message { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal DepositAmount { get; set; } = 0;

        [Required]
        public DepositStatus DepositStatus { get; set; } = DepositStatus.Pending;

        public DateTime? DepositPaidAt { get; set; }

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "vnpay";

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        [Column(TypeName = "text")]
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
