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

    public async Task<(int EarnedPoints, int? CustomerId)> CompleteBookingAndEarnPointsAsync(
        int bookingId,
        decimal vndPerPoint,
        DateTime completedAt)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var booking = await _context.Bookings
                .Include(item => item.BookingAddOns)
                .FirstOrDefaultAsync(item => item.BookingId == bookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            booking.Status = "Completed";
            foreach (var addOn in booking.BookingAddOns)
            {
                addOn.Status = "Completed";
            }

            var existingEarnTransaction = await _context.PointLedgers
                .FirstOrDefaultAsync(item =>
                    item.BookingId == bookingId
                    && item.TransactionType == "Earn");

            if (existingEarnTransaction != null)
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (0, booking.CustomerId);
            }

            if (!booking.CustomerId.HasValue)
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return (0, null);
            }

            var customer = await _context.Customers
                .FirstAsync(item => item.CustomerId == booking.CustomerId.Value);
            var amountSpent = Math.Max(booking.FinalPrice ?? 0, 0);
            var pointMultiplier = await _context.TierRules
                .Where(rule => rule.TierName == (customer.CurrentTier ?? "Member"))
                .Select(rule => (decimal?)rule.PointMultiplier)
                .FirstOrDefaultAsync()
                ?? await _context.TierRules
                    .Where(rule => rule.TierName == "Member")
                    .Select(rule => (decimal?)rule.PointMultiplier)
                    .FirstOrDefaultAsync()
                ?? 1m;
            var earnedPoints = decimal.ToInt32(Math.Floor(
                amountSpent / vndPerPoint * pointMultiplier));

            customer.CurrentPoints = (customer.CurrentPoints ?? 0) + earnedPoints;
            customer.TotalSpent = (customer.TotalSpent ?? 0) + amountSpent;

            _context.PointLedgers.Add(new PointLedger
            {
                CustomerId = customer.CustomerId,
                BookingId = booking.BookingId,
                Points = earnedPoints,
                TransactionType = "Earn",
                Description = $"Earned from completed booking #{booking.BookingId}",
                ExpireDate = completedAt.AddMonths(12),
                CreatedAt = completedAt
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return (earnedPoints, customer.CustomerId);
        });
    }
}
