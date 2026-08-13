using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface ILoyaltyRepository
{
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    Task<int> GetCompletedVisitCountAsync(int customerId);
    Task<(IReadOnlyList<PointLedger> Transactions, int TotalCount)> GetPointTransactionsAsync(
        int customerId,
        int page,
        int pageSize);
    Task<int> CompleteBookingAndEarnPointsAsync(
        int bookingId,
        decimal vndPerPoint,
        DateTime completedAt);
}
