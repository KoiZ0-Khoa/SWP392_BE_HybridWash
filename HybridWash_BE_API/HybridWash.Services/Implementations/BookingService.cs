using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<int> CreateBookingAsync(BookingRequestDTO request, int customerId)
        {
            var booking = new Booking
            {
                CustomerId = customerId,
                VehicleId = request.VehicleId,
                ServiceId = request.ServiceId,
                SlotId = request.SlotId,
                PromotionId = request.PromotionId,
                BookingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)), // Hôm nay
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.CreateBookingAsync(booking);

            return booking.BookingId;
        }
    }
}
