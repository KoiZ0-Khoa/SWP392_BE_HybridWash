using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations
{
    public class StaffRepository : IStaffRepository
    {
        private readonly AutowashContext _context;

        public StaffRepository(AutowashContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetTodayBookingsAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)); // Giờ VN
            return await GetBookingsByDateAsync(today);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateOnly date)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Where(b => b.BookingDate == date)
                .OrderBy(b => b.SlotId)
                .ToListAsync();
        }

        public Task UpdateBookingAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            return Task.CompletedTask;
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.BookingAddOns)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task AddParkingReceiptAsync(ParkingReceipt receipt)
        {
            await _context.ParkingReceipts.AddAsync(receipt);
        }

        public async Task<ParkingReceipt?> GetParkingReceiptByBookingIdAsync(int bookingId)
        {
            return await _context.ParkingReceipts.FirstOrDefaultAsync(pr => pr.BookingId == bookingId);
        }

        public Task UpdateParkingReceiptAsync(ParkingReceipt receipt)
        {
            _context.ParkingReceipts.Update(receipt);
            return Task.CompletedTask;
        }

        public async Task<int> GetActiveWashingsCountAsync(string vehicleType)
        {
            // Calculate number of active washings (Status = "Washing" or "CheckedIn") for the specific vehicle type.
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .CountAsync(b => (b.Status == "Washing" || b.Status == "CheckedIn") 
                                 && (b.Vehicle != null ? b.Vehicle.VehicleType == vehicleType : b.GuestVehicleType == vehicleType));
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
