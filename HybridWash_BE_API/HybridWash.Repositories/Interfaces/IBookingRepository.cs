using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces
{
    public interface IBookingRepository
    {
        Task<Booking> CreateBookingAsync(Booking booking);
    }
}
