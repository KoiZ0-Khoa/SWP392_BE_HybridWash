using HybridWash.Services.DTOs;
using HybridWash.Services.DTOs.Booking;

namespace HybridWash.Services.Interfaces
{
    public interface IBookingService
    {
        Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
        Task<List<BookingDto>> GetBookingsByPhoneAsync(string phone);
        Task<BookingDetailDto> GetBookingByIdAsync(int bookingId);
        Task CancelBookingAsync(int bookingId);
        Task<BookingDto> UpdateBookingStatusAsync(int bookingId, string status);
        Task<PagedResultDto<BookingDto>> GetAdminBookingsAsync(AdminBookingQueryDto query);
    }
}
