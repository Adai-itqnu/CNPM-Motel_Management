using System.ComponentModel.DataAnnotations;

namespace QL_NhaTro_Server.DTOs
{
    public class CreateContractDto
    {
        [Required]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal DepositAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal MonthlyPrice { get; set; }

        public int ElectricityStartIndex { get; set; } = 0;

        public int WaterStartIndex { get; set; } = 0;

        public string? TermsAndConditions { get; set; }
    }

    public class TerminateContractDto
    {
        public string? Reason { get; set; }
    }

    public class ContractResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty; 
        public string UserName { get; set; } = string.Empty; 
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal MonthlyPrice { get; set; }
        public int ElectricityStartIndex { get; set; }
        public int WaterStartIndex { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
