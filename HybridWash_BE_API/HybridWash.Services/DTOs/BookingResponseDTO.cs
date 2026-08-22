using System;
using HybridWash.Services.DTOs.Booking;

namespace HybridWash.Services.DTOs
{
    public class BookingResponseDTO
    {
        public int BookingId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string PaymentStatus { get; set; } = "Unpaid";
        public int SlotId { get; set; }
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public DateOnly BookingDate { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public int? PromotionId { get; set; }
        public string? PromoCode { get; set; }
        public int? RedemptionId { get; set; }
        public AppliedRewardDto? AppliedReward { get; set; }
        public IReadOnlyList<BookingAddOnDto> AddOns { get; set; } = [];
    }
}
