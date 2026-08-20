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

            var existingEarnTransaction = await _context.PointLedgers
                .FirstOrDefaultAsync(item =>
                    item.BookingId == bookingId
                    && item.TransactionType == "Earn");

            if (existingEarnTransaction != null)
            {
                if (booking.Status is not ("Completed" or "CheckedOut"))
                {
                    throw new InvalidOperationException(
                        $"Booking #{bookingId} already has an Earn ledger but its status is {booking.Status}.");
                }

                await transaction.CommitAsync();
                return (0, booking.CustomerId);
            }

            if (booking.Status != "Washing")
            {
                throw new InvalidOperationException(
                    $"Cannot complete booking from status {booking.Status}.");
            }

            booking.Status = "Completed";
            foreach (var addOn in booking.BookingAddOns)
            {
                addOn.Status = "Completed";
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

    public async Task<(int ProcessedCustomers, int ProcessedEarnTransactions, int ExpiredPoints)>
        ExpirePointsAsync(DateTime processedAt)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            var customers = await _context.Customers
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

            var processedCustomers = 0;
            var processedEarnTransactions = 0;
            var expiredPoints = 0;

            foreach (var customer in customers)
            {
                var ledgers = customer.PointLedgers
                    .OrderBy(item => item.CreatedAt)
                    .ThenBy(item => item.TransactionId)
                    .ToList();
                var earns = ledgers
                    .Where(item => item.TransactionType == "Earn" && item.Points > 0)
                    .ToList();

                // Redemptions and previous expirations consume the oldest earned
                // points first, so only the unused part of an Earn can expire.
                var deductionsToAllocate = ledgers
                    .Where(item => item.Points < 0)
                    .Sum(item => -(long)item.Points);
                var remainingPoints = new Dictionary<int, int>();
                foreach (var earn in earns)
                {
                    var consumed = (int)Math.Min(earn.Points, deductionsToAllocate);
                    remainingPoints[earn.TransactionId] = earn.Points - consumed;
                    deductionsToAllocate -= consumed;
                }

                var processedForCustomer = false;
                var availableBalance = Math.Max(customer.CurrentPoints ?? 0, 0);
                foreach (var earn in earns.Where(item =>
                    item.ExpireDate <= processedAt
                    && !ledgers.Any(expiration =>
                        expiration.TransactionType == "Expire"
                        && expiration.SourceTransactionId == item.TransactionId)))
                {
                    var unusedPoints = remainingPoints[earn.TransactionId];
                    var pointsToExpire = Math.Min(unusedPoints, availableBalance);

                    var expiration = new PointLedger
                    {
                        CustomerId = customer.CustomerId,
                        BookingId = earn.BookingId,
                        SourceTransactionId = earn.TransactionId,
                        Points = -pointsToExpire,
                        TransactionType = "Expire",
                        Description = pointsToExpire > 0
                            ? $"Expired unused points from earn transaction #{earn.TransactionId}"
                            : $"Expiration processed for earn transaction #{earn.TransactionId}; no unused points remained",
                        CreatedAt = processedAt
                    };

                    _context.PointLedgers.Add(expiration);
                    ledgers.Add(expiration);
                    availableBalance -= pointsToExpire;
                    expiredPoints += pointsToExpire;
                    processedEarnTransactions++;
                    processedForCustomer = true;
                }

                if (processedForCustomer)
                {
                    customer.CurrentPoints = availableBalance;
                    processedCustomers++;
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (processedCustomers, processedEarnTransactions, expiredPoints);
        });
    }
}
