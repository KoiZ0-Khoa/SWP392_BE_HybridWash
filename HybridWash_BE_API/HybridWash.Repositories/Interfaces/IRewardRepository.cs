using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface IRewardRepository
{
    Task<IReadOnlyList<Reward>> GetAllAsync();
    Task<IReadOnlyList<Reward>> GetActiveAsync(DateTime now);
    Task<Reward?> GetByIdAsync(int rewardId);
    Task<bool> RewardNameExistsAsync(string rewardName, int? excludingRewardId = null);
    Task<bool> ServiceExistsAsync(int serviceId);
    Task AddAsync(Reward reward);
    Task SaveChangesAsync();
    Task<RewardRedemption?> RedeemAsync(int customerId, int rewardId, Guid requestId, DateTime redeemedAt);
    Task<IReadOnlyList<RewardRedemption>> GetRedemptionsAsync(int customerId);
}
