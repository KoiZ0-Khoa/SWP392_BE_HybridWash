using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs
{
    public class CheckInRequestDTO
    {
        [Required(ErrorMessage = "Booking ID là bắt buộc.")]
        public int BookingId { get; set; }

        public IFormFile? IncidentImage1 { get; set; }

        public IFormFile? IncidentImage2 { get; set; }

        public string? StaffNote { get; set; }
    }
}

