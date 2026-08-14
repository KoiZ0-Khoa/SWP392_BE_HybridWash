using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations;

public class TierRepository : ITierRepository
{
    private readonly AutowashContext _context;

    public TierRepository(AutowashContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TierRule>> GetRulesAsync()
    {
        return await _context.TierRules
            .AsNoTracking()
            .OrderBy(rule => rule.Rank)
            .ToListAsync();
    }

    public Task<TierRule?> GetRuleByNameAsync(string tierName, bool tracking = false)
    {
        var query = tracking
            ? _context.TierRules.AsQueryable()
            : _context.TierRules.AsNoTracking();

        return query.FirstOrDefaultAsync(rule => rule.TierName == tierName);
    }

    public Task<Customer?> GetCustomerAsync(int customerId)
    {
        return _context.Customers.FirstOrDefaultAsync(customer =>
            customer.CustomerId == customerId);
    }

    public async Task<IReadOnlyList<int>> GetCustomerIdsForReviewAsync(DateTime? reviewedBefore = null)
    {
        var query = _context.Customers.AsNoTracking();
        if (reviewedBefore.HasValue)
        {
            query = query.Where(customer =>
                !customer.LastTierReviewedAt.HasValue
                || customer.LastTierReviewedAt < reviewedBefore.Value);
        }

        return await query
            .Select(customer => customer.CustomerId)
            .ToListAsync();
    }

    public async Task<(decimal Spend, int Visits)> GetQualifyingMetricsAsync(
        int customerId,
        DateTime from,
        DateTime to)
    {
        var fromDate = DateOnly.FromDateTime(from);
        var toDate = DateOnly.FromDateTime(to);
        var bookings = _context.Bookings
            .AsNoTracking()
            .Where(booking =>
                booking.CustomerId == customerId
                && booking.BookingDate >= fromDate
                && booking.BookingDate <= toDate
                && (booking.Status == "Completed" || booking.Status == "CheckedOut"));

        return (
            await bookings.SumAsync(booking => booking.FinalPrice ?? 0),
            await bookings.CountAsync());
    }

    public void AddHistory(CustomerTierHistory history)
    {
        _context.CustomerTierHistories.Add(history);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
