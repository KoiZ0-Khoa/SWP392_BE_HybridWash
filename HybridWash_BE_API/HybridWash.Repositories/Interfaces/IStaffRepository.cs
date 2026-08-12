using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces
{
    public interface IStaffRepository
    {
        Task<IEnumerable<Booking>> GetTodayBookingsAsync();
        Task<IEnumerable<Booking>> GetBookingsByDateAsync(DateOnly date);
        Task UpdateBookingAsync(Booking booking);
        Task<Booking?> GetBookingByIdAsync(int bookingId);
        Task AddParkingReceiptAsync(ParkingReceipt receipt);
        Task<ParkingReceipt?> GetParkingReceiptByBookingIdAsync(int bookingId);
        Task UpdateParkingReceiptAsync(ParkingReceipt receipt);
        Task SaveChangesAsync();
    }
}
