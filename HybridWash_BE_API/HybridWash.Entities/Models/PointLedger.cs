using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class PointLedger
{
    public int TransactionId { get; set; }

    public int CustomerId { get; set; }

    public int? BookingId { get; set; }

    public int? RewardRedemptionId { get; set; }

    public int Points { get; set; }

    public string? TransactionType { get; set; }

    public string? Description { get; set; }

    public DateTime? ExpireDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual RewardRedemption? RewardRedemption { get; set; }
}
