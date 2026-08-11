using HybridWash.Services.DTOs.TimeSlot;
using HybridWash.Services.Implementations;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeSlotsController : ControllerBase
    {
        private readonly ITimeSlotService _timeSlotService;
        public TimeSlotsController(ITimeSlotService timeSlotsService)
        {
            _timeSlotService = timeSlotsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTimeSlots()
        {
            try
            {
                var slots = await _timeSlotService.GetAllTimeSlotsAsync();
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateTimeSlot([FromBody] CreateTimeSlotDto dto)
        {
            try
            {
                var slot = await _timeSlotService.CreateTimeSlotAsync(dto);
                return CreatedAtAction(nameof(GetTimeSlotById), new { id = slot.SlotId }, slot);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] DateOnly date)
        {
            try
            {
                var slots = await _timeSlotService.GetAvailableSlotsAsync(date);
                return Ok(slots);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTimeSlotById(int id)
        {
            var slot = await _timeSlotService.GetTimeSlotByIdAsync(id);
            if (slot == null) return NotFound();
            return Ok(slot);
        }

    }
}
