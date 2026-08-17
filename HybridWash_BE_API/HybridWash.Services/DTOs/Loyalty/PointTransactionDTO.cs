namespace HybridWash.Services.DTOs.Loyalty;

public class PointTransactionDTO
{
    public int TransactionId { get; set; }
    public int? BookingId { get; set; }
    public int? SourceTransactionId { get; set; }
    public int Points { get; set; }
    public string? TransactionType { get; set; }
    public string? Description { get; set; }
    public DateTime? ExpireDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
