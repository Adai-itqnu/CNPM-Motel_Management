using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaTro_Server.Models
{
    public enum PaymentType
    {
        Deposit,
        MonthlyBill
    }

    public enum PaymentStatus
    {
        Pending,
        Success,
        Failed
    }

    [Table("Payments")]
    public class Payment
    {
        [Key]
        [MaxLength(100)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [MaxLength(50)]
        public string? BillId { get; set; }

        [MaxLength(50)]
        public string? BookingId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal Amount { get; set; }

        [Required]
        public PaymentType PaymentType { get; set; }

        [Required]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        [MaxLength(50)]
        public string Provider { get; set; } = "vnpay";

        [MaxLength(150)]
        public string? ProviderTxnId { get; set; }

        public DateTime? PaymentDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("BillId")]
        public virtual Bill? Bill { get; set; }

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
