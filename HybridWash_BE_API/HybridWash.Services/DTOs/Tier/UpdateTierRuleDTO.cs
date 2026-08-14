using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs.Tier;

public class UpdateTierRuleDTO
{
    [Range(0, 1_000_000_000)]
    public decimal MinimumSpend { get; set; }

    [Range(0, 1_000_000)]
    public int MinimumVisits { get; set; }

    [Range(1, 120)]
    public int EvaluationPeriodMonths { get; set; }

    [Range(1, 365)]
    public int BookingWindowDays { get; set; }

    [Range(typeof(decimal), "0.1", "10")]
    public decimal PointMultiplier { get; set; }

    [MaxLength(500)]
    public string? BenefitDescription { get; set; }

    public bool IsActive { get; set; } = true;
}
