using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.DTOs.Booking
{
    public class BookingDetailDto
    {
        public int BookingId { get; set; }

        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerTier { get; set; }

        public int? VehicleId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string? VehicleType { get; set; }
        // Service
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal ServicePrice { get; set; }
        // Slot
        public int SlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        // Booking
        public DateOnly BookingDate { get; set; }
        public decimal? OriginalPrice { get; set; }
        public decimal? FinalPrice { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal AmountToPay => (FinalPrice ?? OriginalPrice ?? 0) - (DepositAmount ?? 0);
        public int? PromotionId { get; set; }
        public string? PromoCode { get; set; }
        public int? RedemptionId { get; set; }
        public AppliedRewardDto? AppliedReward { get; set; }
        public IReadOnlyList<BookingAddOnDto> AddOns { get; set; } = [];
        public string? Status { get; set; }
        public int? StaffId { get; set; }
        public string? StaffName { get; set; }
        public DateTime? ActualWashTime { get; set; }
        public string? StaffNote { get; set; }
        public string? QrCode { get; set; }
        public string? IncidentImage1 { get; set; }
        public string? IncidentImage2 { get; set; }
        public string? IncidentImage3 { get; set; }
        public string? IncidentImage4 { get; set; }
        public string? IncidentImage5 { get; set; }
        public string? IncidentImage1ApiPath { get; set; }
        public string? IncidentImage2ApiPath { get; set; }
        public string? IncidentImage3ApiPath { get; set; }
        public string? IncidentImage4ApiPath { get; set; }
        public string? IncidentImage5ApiPath { get; set; }
        public ParkingReceiptDto? ParkingReceipt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
