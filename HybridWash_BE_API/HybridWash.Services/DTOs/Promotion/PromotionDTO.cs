namespace HybridWash.Services.DTOs.Promotion;

public class PromotionDTO
{
    public int PromotionId { get; set; }
    public string? PromoCode { get; set; }
    public string PromoName { get; set; } = null!;
    public string? Description { get; set; }
    public string PromoType { get; set; } = null!;
    public string? DiscountType { get; set; }
    public decimal? DiscountValue { get; set; }
    public decimal? MaxDiscount { get; set; }
    public int? ServiceId { get; set; }
    public string TargetTier { get; set; } = null!;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
}
