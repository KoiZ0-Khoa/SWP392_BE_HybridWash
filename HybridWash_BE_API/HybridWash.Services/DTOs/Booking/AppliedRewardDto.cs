namespace HybridWash.Services.DTOs.Booking;

public class AppliedRewardDto
{
    public int RedemptionId { get; set; }
    public int RewardId { get; set; }
    public string RewardName { get; set; } = null!;
    public string RewardType { get; set; } = null!;
    public string? Description { get; set; }
    public int PointsSpent { get; set; }
    public decimal? DiscountValue { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RedeemedAt { get; set; }
    public DateTime? UsedAt { get; set; }
}
