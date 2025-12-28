namespace QL_NhaTro_Server.DTOs
{
    // Booking management DTOs
    public class UpdateBookingStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? AdminNote { get; set; }
    }

    // Contract extension DTO (not in ContractDTOs.cs)
    public class ExtendContractDto
    {
        public int ExtendMonths { get; set; } = 12; // Default 12 months
    }

    // Bill generation DTO
    public class GenerateBillsDto
    {
        public int? Month { get; set; }
        public int? Year { get; set; }
    }

    public class UpdateBillStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateBillMetersDto
    {
        public int ElectricityNewIndex { get; set; }
        public int WaterNewIndex { get; set; }
        public decimal? OtherFees { get; set; }
        public string? Notes { get; set; }
    }

    // Room amenity DTO
    public class AddAmenityDto
    {
        public string AmenityName { get; set; } = string.Empty;
    }

    // Notification DTOs
    public class AdminSendNotificationDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? UserId { get; set; }  // null nếu gửi tất cả
        public string TargetType { get; set; } = "all"; // "all" hoặc "user"
        public bool SendToAll => TargetType == "all"; // Computed property
        public string? Type { get; set; } // notification type from frontend
        public string? Link { get; set; }
    }
}
