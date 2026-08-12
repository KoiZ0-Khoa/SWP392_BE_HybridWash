namespace HybridWash.Entities.Models;

public class BookingAddOn
{
    public int BookingAddOnId { get; set; }

    public int BookingId { get; set; }

    public int ServiceId { get; set; }

    public int? PromotionId { get; set; }

    public int? RedemptionId { get; set; }

    public decimal OriginalPrice { get; set; }

    public decimal FinalPrice { get; set; }

    public string Status { get; set; } = "Pending";

    public DateTime CreatedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;

    public virtual Promotion? Promotion { get; set; }

    public virtual RewardRedemption? Redemption { get; set; }
}
