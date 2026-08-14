namespace HybridWash.Entities.Models;

public class TierRule
{
    public int TierRuleId { get; set; }
    public string TierName { get; set; } = null!;
    public int Rank { get; set; }
    public decimal MinimumSpend { get; set; }
    public int MinimumVisits { get; set; }
    public string QualificationMode { get; set; } = "OR";
    public int EvaluationPeriodMonths { get; set; }
    public int BookingWindowDays { get; set; }
    public decimal PointMultiplier { get; set; }
    public string? BenefitDescription { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
}
