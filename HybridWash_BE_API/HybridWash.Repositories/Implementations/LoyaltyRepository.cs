using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HybridWash.Repositories.Implementations;

public class LoyaltyRepository : ILoyaltyRepository
{
    private readonly AutowashContext _context;

    public LoyaltyRepository(AutowashContext context)
    {
        _context = context;
    }

    public Task<Customer?> GetCustomerByIdAsync(int customerId)
    {
        return _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(customer => customer.CustomerId == customerId);
    }

    public Task<Customer?> GetCustomerForUpdateAsync(int customerId)
    {
        return _context.Customers
            .FirstOrDefaultAsync(customer => customer.CustomerId == customerId);
    }

    public Task<Booking?> GetBookingForUpdateAsync(int bookingId)
    {
        return _context.Bookings
            .Include(booking => booking.BookingAddOns)
            .FirstOrDefaultAsync(booking => booking.BookingId == bookingId);
    }

    public Task<PointLedger?> GetEarnTransactionByBookingIdAsync(int bookingId)
    {
        return _context.PointLedgers.FirstOrDefaultAsync(transaction =>
            transaction.BookingId == bookingId
            && transaction.TransactionType == "Earn");
    }

    public Task<int> GetCompletedVisitCountAsync(int customerId)
    {
        return _context.Bookings.CountAsync(booking =>
            booking.CustomerId == customerId
            && (booking.Status == "Completed" || booking.Status == "CheckedOut"));
    }

    public async Task<(IReadOnlyList<PointLedger> Transactions, int TotalCount)> GetPointTransactionsAsync(
        int customerId,
        int page,
        int pageSize)
    {
        var query = _context.PointLedgers
            .AsNoTracking()
            .Where(transaction => transaction.CustomerId == customerId);

        var totalCount = await query.CountAsync();
        var transactions = await query
            .OrderByDescending(transaction => transaction.CreatedAt)
            .ThenByDescending(transaction => transaction.TransactionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (transactions, totalCount);
    }

    public async Task<IReadOnlyList<Customer>> GetCustomersWithUnprocessedExpiredPointsAsync(
        DateTime processedAt)
    {
        return await _context.Customers
            .Include(customer => customer.PointLedgers)
            .Where(customer => customer.PointLedgers.Any(item =>
                item.TransactionType == "Earn"
                && item.Points > 0
                && item.ExpireDate.HasValue
                && item.ExpireDate <= processedAt
                && !_context.PointLedgers.Any(expiration =>
                    expiration.TransactionType == "Expire"
                    && expiration.SourceTransactionId == item.TransactionId)))
            .ToListAsync();
    }

    public void AddPointLedger(PointLedger transaction)
    {
        _context.PointLedgers.Add(transaction);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public async Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}
