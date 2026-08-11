namespace HybridWash.Services.DTOs.Reward;

public class RewardRedemptionDTO
{
    public int RedemptionId { get; set; }
    public Guid RequestId { get; set; }
    public int RewardId { get; set; }
    public string RewardName { get; set; } = null!;
    public string RewardType { get; set; } = null!;
    public int PointsSpent { get; set; }
    public string Status { get; set; } = null!;
    public DateTime RedeemedAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public int? BookingId { get; set; }
}
