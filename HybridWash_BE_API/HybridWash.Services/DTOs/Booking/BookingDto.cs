using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.DTOs.Booking
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? VehicleId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string? VehicleType { get; set; }
        // Service & Slot
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public int SlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        // Booking info
        public DateOnly BookingDate { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal AmountToPay => (FinalPrice ?? OriginalPrice ?? 0) - (DepositAmount ?? 0);
        public int? PromotionId { get; set; }
        public int? RedemptionId { get; set; }
        public AppliedRewardDto? AppliedReward { get; set; }
        public IReadOnlyList<BookingAddOnDto> AddOns { get; set; } = [];
        public string? QrCode { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
