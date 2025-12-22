using System.ComponentModel.DataAnnotations;

namespace QL_NhaTro_Server.DTOs
{
    // ============ BOOKING DTOs ============
    public class CreateBookingDto
    {
        [Required]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        public DateTime CheckInDate { get; set; }

        public string? Message { get; set; }
    }

    public class ApproveBookingDto
    {
        public string? AdminNote { get; set; }
    }

    public class RejectBookingDto
    {
        [Required]
        public string AdminNote { get; set; } = string.Empty;
    }

    public class BookingResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public string? Message { get; set; }
        public decimal DepositAmount { get; set; }
        public string DepositStatus { get; set; } = string.Empty;
        public DateTime? DepositPaidAt { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============ CONTRACT DTOs ============
    public class CreateContractDto
    {
        [Required]
        public string RoomId { get; set; } = string.Empty;

        [Required]
        public string TenantId { get; set; } = string.Empty;

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
        public string TenantId { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
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
