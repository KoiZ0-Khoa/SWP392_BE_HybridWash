using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Loyalty;
using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HybridWash.Services.Implementations;

public class LoyaltyService : ILoyaltyService
{
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly ITierService _tierService;
    private readonly decimal _vndPerPoint;

    public LoyaltyService(
        ILoyaltyRepository loyaltyRepository,
        ITierService tierService,
        IConfiguration configuration)
    {
        _loyaltyRepository = loyaltyRepository;
        _tierService = tierService;

        if (!decimal.TryParse(configuration["Loyalty:VndPerPoint"], out _vndPerPoint)
            || _vndPerPoint <= 0)
        {
            throw new InvalidOperationException(
                "Loyalty:VndPerPoint must be configured with a value greater than zero.");
        }
    }

    public async Task<LoyaltySummaryDTO?> GetSummaryAsync(int customerId)
    {
        var customer = await _loyaltyRepository.GetCustomerByIdAsync(customerId);
        if (customer == null)
        {
            return null;
        }

        var progress = await _tierService.GetProgressAsync(customerId, DateTime.UtcNow);
        return new LoyaltySummaryDTO
        {
            CurrentPoints = customer.CurrentPoints ?? 0,
            CurrentTier = customer.CurrentTier ?? "Member",
            TotalSpent = customer.TotalSpent ?? 0,
            TotalVisits = await _loyaltyRepository.GetCompletedVisitCountAsync(customerId),
            QualifyingSpend = progress.QualifyingSpend,
            QualifyingVisits = progress.QualifyingVisits,
            BookingWindowDays = progress.BookingWindowDays,
            PointMultiplier = progress.PointMultiplier,
            NextTier = progress.NextTier,
            QualificationMode = progress.QualificationMode,
            SpendRequiredForNextTier = progress.SpendRequiredForNextTier,
            VisitsRequiredForNextTier = progress.VisitsRequiredForNextTier
        };
    }

    public async Task<PointTransactionPageDTO> GetPointTransactionsAsync(
        int customerId,
        int page,
        int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, 100);

        var (transactions, totalCount) = await _loyaltyRepository.GetPointTransactionsAsync(
            customerId,
            page,
            pageSize);

        return new PointTransactionPageDTO
        {
            Items = transactions.Select(transaction => new PointTransactionDTO
            {
                TransactionId = transaction.TransactionId,
                BookingId = transaction.BookingId,
                SourceTransactionId = transaction.SourceTransactionId,
                Points = transaction.Points,
                TransactionType = transaction.TransactionType,
                Description = transaction.Description,
                ExpireDate = transaction.ExpireDate,
                CreatedAt = transaction.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<int> CompleteBookingAndEarnPointsAsync(int bookingId, DateTime completedAt)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(bookingId, 1);
        if (completedAt == default)
        {
            throw new ArgumentException("CompletedAt is required.", nameof(completedAt));
        }

        var result = await _loyaltyRepository.ExecuteInSerializableTransactionAsync(async () =>
        {
            var booking = await _loyaltyRepository.GetBookingForUpdateAsync(bookingId)
                ?? throw new KeyNotFoundException("Booking not found.");

            var existingEarnTransaction = await _loyaltyRepository
                .GetEarnTransactionByBookingIdAsync(bookingId);

            if (existingEarnTransaction != null)
            {
                if (booking.Status is not ("Completed" or "CheckedOut"))
                {
                    throw new InvalidOperationException(
                        $"Booking #{bookingId} already has an Earn ledger but its status is {booking.Status}.");
                }

                return new EarnOperationResult(0, booking.CustomerId, false);
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
                await _loyaltyRepository.SaveChangesAsync();
                return new EarnOperationResult(0, null, false);
            }

            var customer = await _loyaltyRepository.GetCustomerForUpdateAsync(
                    booking.CustomerId.Value)
                ?? throw new KeyNotFoundException("Customer not found.");
            var amountSpent = Math.Max(booking.FinalPrice ?? 0, 0);
            var pointMultiplier = await _tierService.GetPointMultiplierAsync(
                customer.CurrentTier);
            var earnedPoints = decimal.ToInt32(Math.Floor(
                amountSpent / _vndPerPoint * pointMultiplier));

            customer.CurrentPoints = (customer.CurrentPoints ?? 0) + earnedPoints;
            customer.TotalSpent = (customer.TotalSpent ?? 0) + amountSpent;

            _loyaltyRepository.AddPointLedger(new PointLedger
            {
                CustomerId = customer.CustomerId,
                BookingId = booking.BookingId,
                Points = earnedPoints,
                TransactionType = "Earn",
                Description = $"Earned from completed booking #{booking.BookingId}",
                ExpireDate = completedAt.AddMonths(12),
                CreatedAt = completedAt
            });

            await _loyaltyRepository.SaveChangesAsync();
            return new EarnOperationResult(earnedPoints, customer.CustomerId, true);
        });

        if (result.WasEarnProcessed && result.CustomerId.HasValue)
        {
            await _tierService.ReviewAfterCompletedBookingAsync(
                result.CustomerId.Value,
                completedAt);
        }

        return result.EarnedPoints;
    }

    public async Task<PointExpiryResultDTO> ExpirePointsAsync(DateTime processedAt)
    {
        return await _loyaltyRepository.ExecuteInSerializableTransactionAsync(async () =>
        {
            var customers = await _loyaltyRepository
                .GetCustomersWithUnprocessedExpiredPointsAsync(processedAt);
            var result = new PointExpiryResultDTO();

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

                    _loyaltyRepository.AddPointLedger(expiration);
                    ledgers.Add(expiration);
                    availableBalance -= pointsToExpire;
                    result.ExpiredPoints += pointsToExpire;
                    result.ProcessedEarnTransactions++;
                    processedForCustomer = true;
                }

                if (processedForCustomer)
                {
                    customer.CurrentPoints = availableBalance;
                    result.ProcessedCustomers++;
                }
            }

            await _loyaltyRepository.SaveChangesAsync();
            return result;
        });
    }

    private sealed record EarnOperationResult(
        int EarnedPoints,
        int? CustomerId,
        bool WasEarnProcessed);
}
