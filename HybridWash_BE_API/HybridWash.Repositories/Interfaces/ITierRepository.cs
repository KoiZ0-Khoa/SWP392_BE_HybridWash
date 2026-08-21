using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface ITierRepository
{
    Task<IReadOnlyList<TierRule>> GetRulesAsync();
    Task<TierRule?> GetRuleByNameAsync(string tierName, bool tracking = false);
    Task<Customer?> GetCustomerAsync(int customerId);
    Task<bool> HasCustomersInTierAsync(string tierName);
    Task<IReadOnlyList<int>> GetCustomerIdsForReviewAsync(DateTime? reviewedBefore = null);
    Task<(decimal Spend, int Visits)> GetQualifyingMetricsAsync(
        int customerId,
        DateTime from,
        DateTime to);
    void AddHistory(CustomerTierHistory history);
    Task SaveChangesAsync();
}
