using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public class Reward
{
    public int RewardId { get; set; }

    public string RewardName { get; set; } = null!;

    public string? Description { get; set; }

    public string RewardType { get; set; } = null!;

    public int PointCost { get; set; }

    public decimal? DiscountValue { get; set; }

    public int? ServiceId { get; set; }

    public string MinimumTier { get; set; } = "Member";

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Service? Service { get; set; }

    public virtual ICollection<RewardRedemption> Redemptions { get; set; } = new List<RewardRedemption>();
}
