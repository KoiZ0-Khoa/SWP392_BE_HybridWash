using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public Task<int> GetCompletedVisitCountAsync(int customerId)
    {
        return _context.Bookings.CountAsync(booking =>
            booking.CustomerId == customerId && booking.Status == "Completed");
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
}
