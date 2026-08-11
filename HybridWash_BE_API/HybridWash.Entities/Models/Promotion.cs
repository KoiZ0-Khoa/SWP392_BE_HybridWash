using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? PromoCode { get; set; }

    public string PromoName { get; set; } = null!;

    public string? PromoType { get; set; }

    public string? TargetTier { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
