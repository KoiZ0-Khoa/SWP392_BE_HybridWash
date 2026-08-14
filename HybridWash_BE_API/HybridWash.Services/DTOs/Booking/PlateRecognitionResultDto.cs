namespace HybridWash.Services.DTOs.Booking
{
    public class PlateRecognitionResultDto
    {
        /// <summary>Biển số xe đã nhận diện được (null nếu không đọc được)</summary>
        public string? DetectedPlate { get; set; }

        /// <summary>Danh sách booking khớp với biển số</summary>
        public List<BookingDto> Bookings { get; set; } = [];

        /// <summary>Số lượng booking tìm thấy</summary>
        public int BookingCount => Bookings.Count;
    }
}
