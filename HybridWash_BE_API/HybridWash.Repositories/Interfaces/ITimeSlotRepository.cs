using HybridWash.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Repositories.Interfaces
{
    public interface ITimeSlotRepository
    {
        Task<List<TimeSlot>> GetAllTimeSlotsAsync();
        Task<List<TimeSlot>> GetActiveTimeSlotsAsync();
        Task<TimeSlot?> GetTimeSlotByIdAsync(int slotId);
        Task AddTimeSlotAsync(TimeSlot timeSlot);
        Task UpdateTimeSlotAsync(TimeSlot timeSlot);
        Task<bool> ExistsTimeSlotAsync(TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null);

        // Trả về Tuple (số Car, số Bike) đã được book
        Task<(int CarCount, int BikeCount)> CountBookingsInSlotAsync(int slotId, DateOnly bookingDate);
    }
}
