namespace HybridWash.Services.DTOs.Booking
{
    public class PlateRecognitionResultDto
    {
        public string? DetectedPlate { get; set; }

        public List<BookingDto> Bookings { get; set; } = [];

        public int BookingCount => Bookings.Count;
    }
}
