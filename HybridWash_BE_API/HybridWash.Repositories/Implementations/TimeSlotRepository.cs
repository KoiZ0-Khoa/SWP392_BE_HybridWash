using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Repositories.Implementations
{
    public class TimeSlotRepository : ITimeSlotRepository
    {
        private readonly AutowashContext _context;

        public TimeSlotRepository(AutowashContext context)
        {
            _context = context;
        }

        public async Task AddTimeSlotAsync(TimeSlot timeSlot)
        {
            _context.TimeSlots.Add(timeSlot);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTimeSlotAsync(TimeSlot timeSlot)
        {
            _context.TimeSlots.Update(timeSlot);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsTimeSlotAsync(TimeOnly startTime, TimeOnly endTime, int? excludeSlotId = null)
        {
            return await _context.TimeSlots
                .AnyAsync(t => (!excludeSlotId.HasValue || t.SlotId != excludeSlotId.Value)
                            && t.StartTime == startTime
                            && t.EndTime == endTime);
        }

        public async Task<(int CarCount, int BikeCount)> CountBookingsInSlotAsync(int slotId, DateOnly bookingDate)
        {
            var validBookings = _context.Bookings
                .Where(b => b.SlotId == slotId
                         && b.BookingDate == bookingDate
                         && b.Status != "Cancelled"
                         && b.Status != "NoShow");
            var carCount = await validBookings.CountAsync(b =>
                (b.Vehicle != null && b.Vehicle.VehicleType == "Car") || b.GuestVehicleType == "Car");

            var bikeCount = await validBookings.CountAsync(b =>
                (b.Vehicle != null && b.Vehicle.VehicleType == "Bike") || b.GuestVehicleType == "Bike");

            return (carCount, bikeCount);
        }

        public async Task<List<TimeSlot>> GetActiveTimeSlotsAsync()
        {
            return await _context.TimeSlots
               .Where(t => t.IsActive == true)
               .ToListAsync();
        }

        public async Task<List<TimeSlot>> GetAllTimeSlotsAsync()
        {
            return await _context.TimeSlots.ToListAsync();
        }

        public async Task<TimeSlot?> GetTimeSlotByIdAsync(int slotId)
        {
            return await _context.TimeSlots.FindAsync(slotId);
        }
    }
}
