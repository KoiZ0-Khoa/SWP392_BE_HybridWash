using System;

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
        public int SlotId { get; set; }
        public int ServiceId { get; set; }
        public DateOnly BookingDate { get; set; }
    }
}
