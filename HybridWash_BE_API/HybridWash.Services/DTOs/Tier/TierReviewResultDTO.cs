namespace HybridWash.Services.DTOs.Tier;

public class TierReviewResultDTO
{
    public int ReviewedCustomers { get; set; }
    public int UpgradedCustomers { get; set; }
    public int DowngradedCustomers { get; set; }
    public int UnchangedCustomers { get; set; }
}
