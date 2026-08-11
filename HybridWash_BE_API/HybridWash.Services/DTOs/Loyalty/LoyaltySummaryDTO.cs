namespace HybridWash.Services.DTOs.Loyalty;

public class LoyaltySummaryDTO
{
    public int CurrentPoints { get; set; }
    public string CurrentTier { get; set; } = null!;
    public decimal TotalSpent { get; set; }
    public int TotalVisits { get; set; }
}
