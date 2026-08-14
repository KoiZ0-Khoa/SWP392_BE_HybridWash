namespace HybridWash.Services.DTOs.Tier;

public class TierProgressDTO
{
    public decimal QualifyingSpend { get; set; }
    public int QualifyingVisits { get; set; }
    public int BookingWindowDays { get; set; }
    public decimal PointMultiplier { get; set; }
    public string? NextTier { get; set; }
    public string QualificationMode { get; set; } = null!;
    public decimal SpendRequiredForNextTier { get; set; }
    public int VisitsRequiredForNextTier { get; set; }
}
