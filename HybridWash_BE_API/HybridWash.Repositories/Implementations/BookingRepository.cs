using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

        public async Task<bool> HasDuplicateBookingAsync(int? customerId, int slotId, DateOnly bookingDate, string? guestPhone, int? vehicleId, string? guestLicensePlate)
        {
            if (customerId.HasValue)
            {
                return await _context.Bookings.AnyAsync(b =>
                    b.CustomerId == customerId
                    && b.SlotId == slotId
                    && b.BookingDate == bookingDate
                    && b.VehicleId == vehicleId
                    && b.Status != "Cancelled"
                    && b.Status != "NoShow");
            }
            else if (!string.IsNullOrEmpty(guestPhone))
            {
                return await _context.Bookings.AnyAsync(b =>
                    b.GuestPhone == guestPhone
                    && b.SlotId == slotId
                    && b.BookingDate == bookingDate
                    && b.GuestLicensePlate == guestLicensePlate
                    && b.Status != "Cancelled"
                    && b.Status != "NoShow");
            }
            return false;
        }

        // === CRUD ===

        public async Task<Booking> CreateBookingAsync(
            Booking booking,
            int? redemptionId = null,
            int? customerId = null,
            DateTime? usedAt = null)
        {
            if (!redemptionId.HasValue)
            {
                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();
                return booking;
            }

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database
                    .BeginTransactionAsync(IsolationLevel.Serializable);

                var redemption = await _context.RewardRedemptions
                    .FirstOrDefaultAsync(item => item.RedemptionId == redemptionId.Value);
                if (redemption == null
                    || redemption.CustomerId != customerId
                    || redemption.Status != "Issued"
                    || redemption.BookingId.HasValue)
                {
                    throw new InvalidOperationException(
                        "Reward redemption is invalid, already used or does not belong to this customer.");
                }

                await _context.Bookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                redemption.BookingId = booking.BookingId;
                redemption.Status = "Used";
                redemption.UsedAt = usedAt ?? DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return booking;
            });
        }

        public async Task<Booking?> GetBookingByIdWithDetailsAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Staff)
                .Include(b => b.ParkingReceipt)
                    .ThenInclude(p => p.IssueStaff)
                .Include(b => b.Promotion)
                .Include(b => b.BookingAddOns)
                    .ThenInclude(addOn => addOn.Service)
                .Include(b => b.RewardRedemptions)
                    .ThenInclude(redemption => redemption.Reward)
                        .ThenInclude(reward => reward.Service)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);
        }

        public async Task<List<Booking>> GetBookingsByPhoneAsync(string phone)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.BookingAddOns)
                    .ThenInclude(addOn => addOn.Service)
                .Include(b => b.RewardRedemptions)
                    .ThenInclude(redemption => redemption.Reward)
                        .ThenInclude(reward => reward.Service)
                .Where(b => (b.Customer != null && b.Customer.PhoneNumber == phone)
                         || b.GuestPhone == phone)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<List<Booking>> GetBookingsByLicensePlateAsync(string licensePlate)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.BookingAddOns)
                    .ThenInclude(addOn => addOn.Service)
                .Include(b => b.RewardRedemptions)
                    .ThenInclude(redemption => redemption.Reward)
                        .ThenInclude(reward => reward.Service)
                .Where(b => (b.Vehicle != null && b.Vehicle.LicensePlate == licensePlate)
                         || b.GuestLicensePlate == licensePlate)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByQrCodeAsync(string qrCode)
        {
            return await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Staff)
                .Include(b => b.Promotion)
                .Include(b => b.BookingAddOns)
                    .ThenInclude(addOn => addOn.Service)
                .Include(b => b.RewardRedemptions)
                    .ThenInclude(redemption => redemption.Reward)
                        .ThenInclude(reward => reward.Service)
                .FirstOrDefaultAsync(b => b.QrCode == qrCode);
        }

        public IQueryable<Booking> GetBookingsQueryable()
        {
            return _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Vehicle)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Include(b => b.Staff)
                .Include(b => b.BookingAddOns)
                    .ThenInclude(addOn => addOn.Service)
                .Include(b => b.RewardRedemptions)
                    .ThenInclude(redemption => redemption.Reward)
                        .ThenInclude(reward => reward.Service)
                .AsQueryable();
        }

        // === Save ===

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
