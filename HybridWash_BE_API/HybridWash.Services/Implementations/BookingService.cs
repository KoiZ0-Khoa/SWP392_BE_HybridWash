using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IServiceRepository _serviceRepository;

        public BookingService(
            IBookingRepository bookingRepository,
            IServiceRepository serviceRepository)
        {
            _bookingRepository = bookingRepository;
            _serviceRepository = serviceRepository;
        }

        public async Task<int> CreateBookingAsync(BookingRequestDTO request, int customerId)
        {
            var service = await _serviceRepository.GetServiceByIdAsync(request.ServiceId);
            if (service == null || service.IsActive != true)
            {
                throw new ArgumentException("Service not found or inactive.");
            }

            var booking = new Booking
            {
                CustomerId = customerId,
                VehicleId = request.VehicleId,
                ServiceId = request.ServiceId,
                SlotId = request.SlotId,
                PromotionId = request.PromotionId,
                OriginalPrice = service.Price,
                FinalPrice = service.Price,
                BookingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)), // Hôm nay
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepository.CreateBookingAsync(booking);

            return booking.BookingId;
        }
    }
}
