using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class RewardRedemption
{
    public int RedemptionId { get; set; }

    public Guid RequestId { get; set; }

    public int CustomerId { get; set; }

    public int RewardId { get; set; }

    public int PointsSpent { get; set; }

    public string Status { get; set; } = null!;

    public DateTime RedeemedAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public int? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual PointLedger? PointLedger { get; set; }

    public virtual Reward Reward { get; set; } = null!;
}
