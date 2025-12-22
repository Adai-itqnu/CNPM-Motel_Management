using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_NhaTro_Server.Models
{
    public enum ContractStatus
    {
        Active,
        Expired,
        Terminated
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
        public string TenantId { get; set; } = string.Empty;

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

        [ForeignKey("TenantId")]
        public virtual User Tenant { get; set; } = null!;

        public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();
    }
}
