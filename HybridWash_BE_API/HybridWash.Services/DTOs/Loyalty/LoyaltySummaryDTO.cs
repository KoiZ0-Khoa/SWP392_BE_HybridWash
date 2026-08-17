namespace HybridWash.Services.DTOs.Loyalty;

public class LoyaltySummaryDTO
{
    public int CurrentPoints { get; set; }
    public string CurrentTier { get; set; } = null!;
    public decimal TotalSpent { get; set; }
    public int TotalVisits { get; set; }
    public decimal QualifyingSpend { get; set; }
    public int QualifyingVisits { get; set; }
    public int BookingWindowDays { get; set; }
    public decimal PointMultiplier { get; set; }
    public string? NextTier { get; set; }
    public string QualificationMode { get; set; } = null!;
    public decimal SpendRequiredForNextTier { get; set; }
    public int VisitsRequiredForNextTier { get; set; }
}
