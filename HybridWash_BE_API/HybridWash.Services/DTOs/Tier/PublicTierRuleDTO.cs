namespace HybridWash.Services.DTOs.Tier;

public class PublicTierRuleDTO
{
    public string TierName { get; set; } = null!;
    public int Rank { get; set; }
    public decimal MinimumSpend { get; set; }
    public int MinimumVisits { get; set; }
    public string QualificationMode { get; set; } = null!;
    public int EvaluationPeriodMonths { get; set; }
    public int BookingWindowDays { get; set; }
    public decimal PointMultiplier { get; set; }
    public string? BenefitDescription { get; set; }
}
