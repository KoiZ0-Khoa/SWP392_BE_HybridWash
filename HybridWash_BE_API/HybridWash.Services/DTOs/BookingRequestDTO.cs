namespace HybridWash.Services.DTOs
{
    public class BookingRequestDTO
    {
        public int VehicleId { get; set; }
        public int ServiceId { get; set; }
        public int SlotId { get; set; }
        public int? PromotionId { get; set; }
    }
}
