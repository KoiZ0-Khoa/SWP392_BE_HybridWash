using HybridWash.Services.DTOs.Booking;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto dto)
        {
            try
            {
                var booking = await _bookingService.CreateBookingAsync(dto);
                return CreatedAtAction(nameof(GetBookingById),
                    new { bookingId = booking.BookingId }, booking);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                if (ex.Message.Contains("fully booked") || ex.Message.Contains("already have"))
                    return Conflict(new { Message = ex.Message });
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("search")]
        [Authorize]
        public async Task<IActionResult> GetBookingsByPhone([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                    return BadRequest(new { Message = "Phone number is required" });

                var bookings = await _bookingService.GetBookingsByPhoneAsync(phone);
                return Ok(new { Success = true, Data = bookings });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{bookingId}")]
        [Authorize]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByIdAsync(bookingId);
                return Ok(new { Success = true, Data = booking });
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpGet("search-by-plate")]
        [Authorize]
        public async Task<IActionResult> GetBookingsByLicensePlate([FromQuery] string licensePlate)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(licensePlate))
                    return BadRequest(new { Message = "License plate is required" });

                var bookings = await _bookingService.GetBookingsByLicensePlateAsync(licensePlate);
                return Ok(new { Success = true, Data = bookings });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("checkin/{qrCode}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetBookingByQrCode(string qrCode)
        {
            try
            {
                var booking = await _bookingService.GetBookingByQrCodeAsync(qrCode);
                return Ok(new { Success = true, Data = booking });
            }
            catch (Exception ex)
            {
                return NotFound(new { Message = ex.Message });
            }
        }

        [HttpPut("{bookingId}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                var message = await _bookingService.CancelBookingAsync(bookingId);
                return Ok(new { Success = true, Message = message });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("{bookingId}/status")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, [FromQuery] string status)
        {
            try
            {
                var booking = await _bookingService.UpdateBookingStatusAsync(bookingId, status);
                return Ok(new { Success = true, Data = booking });
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("not found"))
                    return NotFound(new { Message = ex.Message });
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> GetAdminBookings(
            [FromQuery] AdminBookingQueryDto query)
        {
            try
            {
                var result = await _bookingService.GetAdminBookingsAsync(query);
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("scan-plate")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ScanPlate(IFormFile image)
        {
            try
            {
                if (image == null || image.Length == 0)
                    return BadRequest(new { Message = "Image file is required" });

                // Giới hạn file 5MB
                if (image.Length > 5 * 1024 * 1024)
                    return BadRequest(new { Message = "Image must be smaller than 5MB" });

                using var stream = image.OpenReadStream();
                var result = await _bookingService.ScanPlateAsync(stream);

                if (result.DetectedPlate == null)
                    return Ok(new { Success = true, Message = "Could not detect license plate", Data = result });

                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
