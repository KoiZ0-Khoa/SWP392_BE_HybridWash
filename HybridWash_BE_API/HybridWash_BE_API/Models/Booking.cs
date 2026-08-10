using System;
using System.Collections.Generic;

namespace HybridWash_BE_API.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public int CustomerId { get; set; }

    public int VehicleId { get; set; }

    public int ServiceId { get; set; }

    public int? PromotionId { get; set; }

    public int SlotId { get; set; }

    public int? StaffId { get; set; }

    public DateOnly BookingDate { get; set; }

    public decimal? OriginalPrice { get; set; }

    public decimal? FinalPrice { get; set; }

    public DateTime? ActualWashTime { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<PointLedger> PointLedgers { get; set; } = new List<PointLedger>();

    public virtual Promotion? Promotion { get; set; }

    public virtual Service Service { get; set; } = null!;

    public virtual TimeSlot Slot { get; set; } = null!;

    public virtual Staff? Staff { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;
}
