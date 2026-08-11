using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.DTOs.Booking
{
    public class CreateBookingDto
    {
        public int? CustomerId { get; set; }
        public int? VehicleId { get; set; }

        public string? GuestName { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestLicensePlate { get; set; }
        public string? GuestVehicleType { get; set; }

        public int ServiceId { get; set; }
        public int SlotId { get; set; }
        public DateOnly BookingDate { get; set; }
        public int? PromotionId { get; set; }
    }
}
