using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface ILoyaltyRepository
{
    Task<Customer?> GetCustomerByIdAsync(int customerId);
    Task<Customer?> GetCustomerForUpdateAsync(int customerId);
    Task<Booking?> GetBookingForUpdateAsync(int bookingId);
    Task<PointLedger?> GetEarnTransactionByBookingIdAsync(int bookingId);
    Task<int> GetCompletedVisitCountAsync(int customerId);
    Task<(IReadOnlyList<PointLedger> Transactions, int TotalCount)> GetPointTransactionsAsync(
        int customerId,
        int page,
        int pageSize);
    Task<IReadOnlyList<Customer>> GetCustomersWithUnprocessedExpiredPointsAsync(
        DateTime processedAt);
    void AddPointLedger(PointLedger transaction);
    Task SaveChangesAsync();
    Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> operation);
}
