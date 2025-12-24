using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaTro_Server.Models
{
    public enum ContractStatus
    {
        Draft,      // Created after payment, waiting for check-in
        Active,     // After check-in
        Expired,
        Terminated,
        Cancelled   // If no check-in by check-in date
    }


    [Table("Contracts")]
    public class Contract
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

        [MaxLength(50)]
        public string? BookingId { get; set; }  // Reference to booking


        [Required]
        [Column(TypeName = "date")]
        public DateTime StartDate { get; set; }

        [Column(TypeName = "date")]
        public DateTime? EndDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal DepositAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(15,2)")]
        public decimal MonthlyPrice { get; set; }

        public int ElectricityStartIndex { get; set; } = 0;

        public int WaterStartIndex { get; set; } = 0;

        [Column(TypeName = "text")]
        public string? TermsAndConditions { get; set; }

        [Required]
        public ContractStatus Status { get; set; } = ContractStatus.Active;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("RoomId")]
        public virtual Room Room { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        [ForeignKey("BookingId")]
        public virtual Booking? Booking { get; set; }


        public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}
