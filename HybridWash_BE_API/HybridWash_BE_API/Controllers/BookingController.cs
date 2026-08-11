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

        [HttpGet("customer/{customerId}")]
        [Authorize]
        public async Task<IActionResult> GetBookingsByCustomer(int customerId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByCustomerIdAsync(customerId);
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

        [HttpPut("{bookingId}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            try
            {
                await _bookingService.CancelBookingAsync(bookingId);
                return Ok(new { Success = true, Message = "Booking cancelled successfully" });
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
    }
}
