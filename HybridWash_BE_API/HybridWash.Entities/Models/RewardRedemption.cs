using System;

namespace HybridWash.Entities.Models;

public class RewardRedemption
{
    public int RedemptionId { get; set; }

    public Guid RequestId { get; set; }

    public int CustomerId { get; set; }

    public int RewardId { get; set; }

    public int PointsSpent { get; set; }

    public string Status { get; set; } = "Issued";

    public DateTime RedeemedAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public int? BookingId { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Reward Reward { get; set; } = null!;

    public virtual Booking? Booking { get; set; }

    public virtual PointLedger? PointTransaction { get; set; }
}
