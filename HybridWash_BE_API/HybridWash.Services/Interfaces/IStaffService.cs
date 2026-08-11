using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<BookingResponseDTO>> GetTodayBookingsAsync();
        Task<DailyHistoryResponseDTO> GetDailyHistoryAsync(DateOnly date);
        Task<BookingResponseDTO?> GetBookingByQrCodeAsync(string qrCode);
        Task<bool> ConfirmBookingAsync(BookingIdRequestDTO request);
        Task<bool> CheckInAsync(BookingIdRequestDTO request);
        Task<bool> IssueReceiptAsync(IssueReceiptRequestDTO request, int staffId);
        Task<bool> VerifyReceiptAsync(VerifyReceiptRequestDTO request, int staffId);
        Task<bool> CheckOutAsync(BookingIdRequestDTO request, int staffId);
    }
}
