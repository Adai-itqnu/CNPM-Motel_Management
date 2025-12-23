using QL_NhaTro_Server.Models;

namespace QL_NhaTro_Server.DTOs
{
    // DTO for creating a new booking with deposit payment
    public class CreateBookingDto
    {
        public string RoomId { get; set; } = string.Empty;
        public DateTime CheckInDate { get; set; }
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }


    // Response after creating booking with VNPAY URL
    public class BookingResponseDto
    {
        public string BookingId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public decimal DepositAmount { get; set; }
    }
}
