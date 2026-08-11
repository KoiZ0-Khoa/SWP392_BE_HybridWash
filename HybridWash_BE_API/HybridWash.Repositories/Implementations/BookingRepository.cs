using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;

namespace HybridWash.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AutowashContext _context;

        public BookingRepository(AutowashContext context)
        {
            _context = context;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }
    }
}
