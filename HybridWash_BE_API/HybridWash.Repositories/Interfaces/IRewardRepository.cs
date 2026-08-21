using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface IRewardRepository
{
    Task<IReadOnlyList<Reward>> GetAllAsync();
    Task<IReadOnlyList<Reward>> GetActiveAsync(DateTime now);
    Task<Reward?> GetByIdAsync(int rewardId);
    Task<Reward?> GetByIdForUpdateAsync(int rewardId);
    Task<Customer?> GetCustomerForUpdateAsync(int customerId);
    Task<RewardRedemption?> GetRedemptionByRequestIdAsync(Guid requestId);
    Task<bool> RewardNameExistsAsync(string rewardName, int? excludingRewardId = null);
    Task<bool> ActiveServiceExistsAsync(int serviceId);
    Task AddAsync(Reward reward);
    void AddRedemption(RewardRedemption redemption);
    void AddPointLedger(PointLedger transaction);
    Task SaveChangesAsync();
    Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> operation);
    Task<RewardRedemption?> GetRedemptionByIdAsync(int redemptionId);
    Task<IReadOnlyList<RewardRedemption>> GetRedemptionsAsync(int customerId);
}
