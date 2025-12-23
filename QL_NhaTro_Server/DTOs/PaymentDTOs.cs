using System.ComponentModel.DataAnnotations;

namespace QL_NhaTro_Server.DTOs
{
    // ============ PAYMENT DTOs ============
    
    // For creating deposit payment
    public class CreateDepositDto
    {
        public string BookingId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string RoomId { get; set; } = string.Empty;
        public decimal DepositAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }
    
    // For creating monthly bill payment
    public class CreatePaymentDto
    {
        public string? BillId { get; set; }
        public string? BookingId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public string PaymentType { get; set; } = string.Empty; // "deposit" or "monthly_bill"

        [Required]
        public string PaymentMethod { get; set; } = "vnpay";
    }

    // Payment response
    public class PaymentResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string? BillId { get; set; }
        public string? BookingId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string? ProviderTxnId { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // VNPAY callback from payment gateway
    public class VNPayCallbackDto
    {
        public string vnp_TxnRef { get; set; } = string.Empty;
        public string vnp_Amount { get; set; } = string.Empty;
        public string vnp_OrderInfo { get; set; } = string.Empty;
        public string vnp_ResponseCode { get; set; } = string.Empty;
        public string vnp_TransactionNo { get; set; } = string.Empty;
        public string vnp_BankCode { get; set; } = string.Empty;
        public string vnp_PayDate { get; set; } = string.Empty;
        public string vnp_SecureHash { get; set; } = string.Empty;
    }

    // Payment result after processing
    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string ContractId { get; set; } = string.Empty;
    }
}
