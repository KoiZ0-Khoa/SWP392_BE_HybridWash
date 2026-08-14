namespace HybridWash.Services.DTOs.Booking;

public class BookingAddOnDto
{
    public int BookingAddOnId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public int? PromotionId { get; set; }
    public int? RedemptionId { get; set; }
    public int? RewardId { get; set; }
    public string? RewardName { get; set; }
    public string? RewardType { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public string Status { get; set; } = null!;
}
