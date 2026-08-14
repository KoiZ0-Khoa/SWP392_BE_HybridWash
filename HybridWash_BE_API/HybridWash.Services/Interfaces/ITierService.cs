using HybridWash.Services.DTOs.Tier;

namespace HybridWash.Services.Interfaces;

public interface ITierService
{
    Task<IReadOnlyList<TierRuleDTO>> GetRulesAsync();
    Task<TierRuleDTO?> UpdateRuleAsync(string tierName, UpdateTierRuleDTO request);
    Task ReviewAfterCompletedBookingAsync(int customerId, DateTime reviewedAt);
    Task<TierReviewResultDTO> RunMonthlyReviewAsync(DateTime reviewedAt, bool onlyDueCustomers);
    Task<TierProgressDTO> GetProgressAsync(int customerId, DateTime at);
    Task<int> GetBookingWindowDaysAsync(string? tierName);
    Task<decimal> GetPointMultiplierAsync(string? tierName);
}
