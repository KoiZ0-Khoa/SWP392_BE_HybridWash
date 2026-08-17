using HybridWash.Services.DTOs.Loyalty;

namespace HybridWash.Services.Interfaces;

public interface ILoyaltyService
{
    Task<LoyaltySummaryDTO?> GetSummaryAsync(int customerId);
    Task<PointTransactionPageDTO> GetPointTransactionsAsync(int customerId, int page, int pageSize);
    Task<int> CompleteBookingAndEarnPointsAsync(int bookingId, DateTime completedAt);
    Task<PointExpiryResultDTO> ExpirePointsAsync(DateTime processedAt);
}
