using HybridWash.Services.DTOs.Reward;

namespace HybridWash.Services.Interfaces;

public interface IRewardService
{
    Task<IReadOnlyList<RewardDTO>> GetAllAsync();
    Task<RewardDTO?> GetByIdAsync(int rewardId);
    Task<RewardDTO> CreateAsync(UpsertRewardDTO request);
    Task<bool> UpdateAsync(int rewardId, UpsertRewardDTO request);
    Task<bool> DeactivateAsync(int rewardId);
    Task<IReadOnlyList<RewardDTO>> GetEligibleAsync(int customerId);
    Task<RewardRedemptionDTO> RedeemAsync(int customerId, int rewardId, Guid requestId);
    Task<IReadOnlyList<RewardRedemptionDTO>> GetRedemptionsAsync(int customerId);
}
