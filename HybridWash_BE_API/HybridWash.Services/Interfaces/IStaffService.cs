using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<BookingResponseDTO>> GetTodayBookingsAsync();
        Task<DailyHistoryResponseDTO> GetDailyHistoryAsync(DateOnly date);
        Task<bool> ConfirmBookingAsync(BookingIdRequestDTO request);
        Task<string> CheckInAsync(CheckInRequestDTO request);
        Task<bool> IssueReceiptAsync(IssueReceiptRequestDTO request, int staffId);
        Task<string> CheckOutAsync(BookingIdRequestDTO request, int staffId);
    }
}
