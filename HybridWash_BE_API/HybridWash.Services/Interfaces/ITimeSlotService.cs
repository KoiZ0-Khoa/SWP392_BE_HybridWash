using HybridWash.Entities.Models;
using HybridWash.Services.DTOs.TimeSlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.Interfaces
{
    public interface ITimeSlotService
    {
        Task<List<TimeSlotDto>> GetAllTimeSlotsAsync();
        Task<TimeSlotDto> CreateTimeSlotAsync(CreateTimeSlotDto dto);
        Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(DateOnly date);
        Task<TimeSlotDto?> GetTimeSlotByIdAsync(int slotId);
        Task<TimeSlotDto> ToggleSlotStatusAsync(int slotId, bool isActive);
    }
}
