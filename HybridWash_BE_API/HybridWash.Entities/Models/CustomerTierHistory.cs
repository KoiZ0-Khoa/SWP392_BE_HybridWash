namespace HybridWash.Entities.Models;

public class CustomerTierHistory
{
    public int TierHistoryId { get; set; }
    public int CustomerId { get; set; }
    public string PreviousTier { get; set; } = null!;
    public string NewTier { get; set; } = null!;
    public decimal QualifyingSpend { get; set; }
    public int QualifyingVisits { get; set; }
    public string ReviewType { get; set; } = null!;
    public string? Reason { get; set; }
    public DateTime ReviewedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
