    using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaTro_Server.Models
{
    public enum BillStatus
    {
        Pending,
        Paid,
        PartiallyPaid,
        Overdue
    }

    [Table("Bills")]
    public class Bill
    {
        [Key]
        [MaxLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(50)]
        public string ContractId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int Month { get; set; }

        [Required]
        public int Year { get; set; }

        // Electricity
        public int ElectricityOldIndex { get; set; } = 0;
        public int ElectricityNewIndex { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal ElectricityPrice { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal ElectricityTotal { get; set; } = 0;

        // Water
        public int WaterOldIndex { get; set; } = 0;
        public int WaterNewIndex { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal WaterPrice { get; set; } = 0;

        [Column(TypeName = "decimal(15,2)")]
        public decimal WaterTotal { get; set; } = 0;

        // Pricing
        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal RoomPrice { get; set; }

        [Column(TypeName = "decimal(15,2)")]
        public decimal OtherFees { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal TotalAmount { get; set; }

        // Pro-rata calculation fields
        public int DaysInMonth { get; set; } = 30;  // Tổng số ngày trong tháng
        public int DaysRented { get; set; } = 30;   // Số ngày thực tế ở

        // Trạng thái đã gửi hóa đơn cho người thuê
        public bool IsSent { get; set; } = false;

        [Required]
        public BillStatus Status { get; set; } = BillStatus.Pending;

        public DateTime? PaymentDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DueDate { get; set; }

        [Column(TypeName = "text")]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ContractId")]
        public virtual Contract Contract { get; set; } = null!;

        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
