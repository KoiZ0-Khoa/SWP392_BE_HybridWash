using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
        // Conflict checks
        Task<int> CountBookingsInSlotByTypeAsync(int slotId, DateOnly bookingDate, string vehicleType);
        Task<bool> HasDuplicateBookingAsync(int? customerId, int slotId, DateOnly bookingDate, string? guestPhone);
        // CRUD
        Task<Booking> CreateBookingAsync(
            Booking booking,
            int? redemptionId = null,
            int? customerId = null,
            DateTime? usedAt = null);
        Task<Booking?> GetBookingByIdWithDetailsAsync(int bookingId);
        Task<List<Booking>> GetBookingsByPhoneAsync(string phone);
        Task<List<Booking>> GetBookingsByLicensePlateAsync(string licensePlate);
        Task<Booking?> GetBookingByQrCodeAsync(string qrCode);
        IQueryable<Booking> GetBookingsQueryable();
        // Save
        Task SaveChangesAsync();
    }
}
