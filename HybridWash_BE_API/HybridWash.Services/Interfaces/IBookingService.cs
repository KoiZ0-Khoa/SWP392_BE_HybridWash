using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces
{
    public interface IBookingService
    {
        Task<int> CreateBookingAsync(BookingRequestDTO request, int customerId);
    }
}
