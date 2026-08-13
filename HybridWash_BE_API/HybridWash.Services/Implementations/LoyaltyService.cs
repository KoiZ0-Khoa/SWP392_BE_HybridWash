using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Loyalty;
using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HybridWash.Services.Implementations;

public class LoyaltyService : ILoyaltyService
{
    private readonly ILoyaltyRepository _loyaltyRepository;
    private readonly decimal _vndPerPoint;

    public LoyaltyService(
        ILoyaltyRepository loyaltyRepository,
        IConfiguration configuration)
    {
        _loyaltyRepository = loyaltyRepository;

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

        return new LoyaltySummaryDTO
        {
            CurrentPoints = customer.CurrentPoints ?? 0,
            CurrentTier = customer.CurrentTier ?? "Member",
            TotalSpent = customer.TotalSpent ?? 0,
            TotalVisits = await _loyaltyRepository.GetCompletedVisitCountAsync(customerId)
        };
    }

    public async Task<PointTransactionPageDTO> GetPointTransactionsAsync(
        int customerId,
        int page,
        int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

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
                Points = transaction.Points,
                TransactionType = transaction.TransactionType,
                ExpireDate = transaction.ExpireDate,
                CreatedAt = transaction.CreatedAt
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public Task<int> CompleteBookingAndEarnPointsAsync(int bookingId, DateTime completedAt)
    {
        return _loyaltyRepository.CompleteBookingAndEarnPointsAsync(
            bookingId,
            _vndPerPoint,
            completedAt);
    }
}
