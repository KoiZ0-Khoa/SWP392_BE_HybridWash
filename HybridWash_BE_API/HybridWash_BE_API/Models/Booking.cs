using System;
using System.Collections.Generic;

namespace HybridWash_BE_API.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int CustomerId { get; set; }

    public int VehicleId { get; set; }

    public int? PromotionId { get; set; }

    public DateTime BookingTime { get; set; }

    public DateTime? ActualWashTime { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<PointLedger> PointLedgers { get; set; } = new List<PointLedger>();

    public virtual Promotion? Promotion { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
}
