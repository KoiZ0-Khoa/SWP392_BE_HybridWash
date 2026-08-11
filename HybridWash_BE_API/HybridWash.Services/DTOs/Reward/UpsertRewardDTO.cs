using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs.Reward;

public class UpsertRewardDTO
{
    [Required, MaxLength(100)]
    public string RewardName { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string RewardType { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int PointCost { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal? DiscountValue { get; set; }

    public int? ServiceId { get; set; }

    [Required]
    public string MinimumTier { get; set; } = "Member";

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}
