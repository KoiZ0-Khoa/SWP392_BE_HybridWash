using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class Reward
{
    public int RewardId { get; set; }

    public string RewardName { get; set; } = null!;

    public string? Description { get; set; }

    public string RewardType { get; set; } = null!;

    public int PointCost { get; set; }

    public decimal? DiscountValue { get; set; }

    public int? ServiceId { get; set; }

    public string MinimumTier { get; set; } = null!;

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();

    public virtual Service? Service { get; set; }
}
