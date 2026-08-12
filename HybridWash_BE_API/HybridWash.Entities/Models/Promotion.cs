using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class Promotion
{
    public int PromotionId { get; set; }

    public string? PromoCode { get; set; }

    public string PromoName { get; set; } = null!;

    public string? Description { get; set; }

    public string? PromoType { get; set; }

    public string? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    public decimal? MaxDiscount { get; set; }

    public int? ServiceId { get; set; }

    public string? TargetTier { get; set; }

    public DateTime? ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }

    public bool IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<BookingAddOn> BookingAddOns { get; set; } = new List<BookingAddOn>();

    public virtual Service? Service { get; set; }
}
