using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AutowashContext _context;

        public BookingRepository(AutowashContext context)
        {
            _context = context;
        }


        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _context.Customers.FindAsync(customerId);
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
        {
            return await _context.Vehicles.FindAsync(vehicleId);
        }


        public async Task<int> CountBookingsInSlotByTypeAsync(int slotId, DateOnly bookingDate, string vehicleType)
        {
            return await _context.Bookings
                .CountAsync(b => b.SlotId == slotId
                              && b.BookingDate == bookingDate
                              && b.Status != "Cancelled"
                              && b.Status != "NoShow"
                              && (
                                  (b.Vehicle != null && b.Vehicle.VehicleType == vehicleType)
                                  || (b.Vehicle == null && b.GuestVehicleType == vehicleType)
                              ));
        }

        public async Task<bool> HasDuplicateBookingAsync(int? customerId, int slotId, DateOnly bookingDate, string? guestPhone)
        {
            if (customerId.HasValue)
            {
                return await _context.Bookings.AnyAsync(b =>
                    b.CustomerId == customerId
                    && b.SlotId == slotId
                    && b.BookingDate == bookingDate
                    && b.Status != "Cancelled"
                    && b.Status != "NoShow");
            }
            else if (!string.IsNullOrEmpty(guestPhone))
            {
                return await _context.Bookings.AnyAsync(b =>
                    b.GuestPhone == guestPhone
                    && b.SlotId == slotId
                    && b.BookingDate == bookingDate
                    && b.Status != "Cancelled"
                    && b.Status != "NoShow");
            }
            return false;
        }

        // === CRUD ===

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> GetBookingByIdWithDetailsAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Staff)
                .Include(b => b.Promotion)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<List<Booking>> GetBookingsByCustomerIdAsync(int customerId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Where(b => b.CustomerId == customerId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public IQueryable<Booking> GetBookingsQueryable()
        {
            return _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Staff)
                .AsQueryable();
        }

        // === Save ===

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
