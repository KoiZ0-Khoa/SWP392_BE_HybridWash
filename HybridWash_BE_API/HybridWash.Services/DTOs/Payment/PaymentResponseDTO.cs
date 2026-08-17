namespace HybridWash.Services.DTOs.Payment;

public class PaymentResponseDTO
{
    public long OrderCode { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Bin { get; set; } = string.Empty;
    public string QrCode { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string? PaymentLinkId { get; set; }
    public string? Status { get; set; }
    public string? QrImageUrl { get; set; }
}
