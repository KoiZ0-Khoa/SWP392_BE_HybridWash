using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs.Promotion;

public class UpsertPromotionDTO
{
    [MaxLength(50)]
    public string? PromoCode { get; set; }

    [Required, MaxLength(100)]
    public string PromoName { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string PromoType { get; set; } = null!;

    public string? DiscountType { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal? DiscountValue { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999")]
    public decimal? MaxDiscount { get; set; }

    public int? ServiceId { get; set; }

    [Required]
    public string TargetTier { get; set; } = null!;

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}
