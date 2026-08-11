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

        public async Task<Booking?> GetActiveBookingByQrCodeAsync(string qrCode)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.Customer)
                .FirstOrDefaultAsync(b => b.Vehicle != null && b.Vehicle.QrCode == qrCode 
                    && b.BookingDate == today 
                    && b.Status != "Cancelled" && b.Status != "NoShow" && b.Status != "CheckedOut");
        }

        public async Task<Booking?> GetPendingBookingByQrCodeAsync(string qrCode)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .FirstOrDefaultAsync(b => b.Vehicle != null && b.Vehicle.QrCode == qrCode 
                    && b.BookingDate == today 
                    && (b.Status == "Pending" || b.Status == "Confirmed"));
        }

        public Task UpdateBookingAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            return Task.CompletedTask;
        }

        public async Task<Booking?> GetBookingByIdAsync(int bookingId)
        {
            return await _context.Bookings.FindAsync(bookingId);
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

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
