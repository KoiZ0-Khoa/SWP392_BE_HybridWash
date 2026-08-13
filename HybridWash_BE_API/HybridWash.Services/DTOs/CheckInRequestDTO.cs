using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs
{
    public class CheckInRequestDTO
    {
        [Required(ErrorMessage = "Booking ID là bắt buộc.")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Vui lòng tải lên ảnh tình trạng xe thứ 1.")]
        public IFormFile IncidentImage1 { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng tải lên ảnh tình trạng xe thứ 2.")]
        public IFormFile IncidentImage2 { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập ghi chú tình trạng xe.")]
        public string StaffNote { get; set; } = null!;
    }
}
