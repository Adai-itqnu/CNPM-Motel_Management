using System.ComponentModel.DataAnnotations;

namespace QL_NhaTro_Server.DTOs
{
    // ============ BILL DTOs ============
    public class CreateBillDto
    {
        [Required]
        public string ContractId { get; set; } = string.Empty;

        [Required]
        [Range(1, 12)]
        public int Month { get; set; }

        [Required]
        [Range(2000, 2100)]
        public int Year { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int ElectricityNewIndex { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int WaterNewIndex { get; set; }

        [Range(0, double.MaxValue)]
        public decimal OtherFees { get; set; } = 0;

        public DateTime? DueDate { get; set; }

        public string? Notes { get; set; }
    }

    public class UpdateBillDto
    {
        [Range(0, int.MaxValue)]
        public int? ElectricityNewIndex { get; set; }

        [Range(0, int.MaxValue)]
        public int? WaterNewIndex { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? OtherFees { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Notes { get; set; }
    }

    public class BillResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }

        public int ElectricityOldIndex { get; set; }
        public int ElectricityNewIndex { get; set; }
        public decimal ElectricityPrice { get; set; }
        public decimal ElectricityTotal { get; set; }

        public int WaterOldIndex { get; set; }
        public int WaterNewIndex { get; set; }
        public decimal WaterPrice { get; set; }
        public decimal WaterTotal { get; set; }

        public decimal RoomPrice { get; set; }
        public decimal OtherFees { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
