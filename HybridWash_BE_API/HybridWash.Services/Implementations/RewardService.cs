using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Reward;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations;

public class RewardService : IRewardService
{
    private readonly IRewardRepository _rewardRepository;
    private readonly ILoyaltyRepository _loyaltyRepository;

    public RewardService(IRewardRepository rewardRepository, ILoyaltyRepository loyaltyRepository)
    {
        _rewardRepository = rewardRepository;
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<IReadOnlyList<RewardDTO>> GetAllAsync()
    {
        return (await _rewardRepository.GetAllAsync()).Select(reward => Map(reward, false)).ToList();
    }

    public async Task<RewardDTO?> GetByIdAsync(int rewardId)
    {
        var reward = await _rewardRepository.GetByIdAsync(rewardId);
        return reward == null ? null : Map(reward, false);
    }

    public async Task<RewardDTO> CreateAsync(UpsertRewardDTO request)
    {
        var rewardName = request.RewardName.Trim();
        if (await _rewardRepository.RewardNameExistsAsync(rewardName))
        {
            throw new InvalidOperationException("Reward name already exists.");
        }

        await ValidateRequestAsync(request);

        var reward = new Reward
        {
            RewardName = rewardName,
            Description = request.Description?.Trim(),
            RewardType = BenefitRules.NormalizeType(request.RewardType),
            PointCost = request.PointCost,
            DiscountValue = request.DiscountValue,
            ServiceId = request.ServiceId,
            MinimumTier = BenefitRules.NormalizeTier(request.MinimumTier, allowAll: false),
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _rewardRepository.AddAsync(reward);
        return Map(reward, false);
    }

    public async Task<bool> UpdateAsync(int rewardId, UpsertRewardDTO request)
    {
        var reward = await _rewardRepository.GetByIdAsync(rewardId);
        if (reward == null)
        {
            return false;
        }

        var rewardName = request.RewardName.Trim();
        if (await _rewardRepository.RewardNameExistsAsync(rewardName, rewardId))
        {
            throw new InvalidOperationException("Reward name already exists.");
        }

        await ValidateRequestAsync(request);

        reward.RewardName = rewardName;
        reward.Description = request.Description?.Trim();
        reward.RewardType = BenefitRules.NormalizeType(request.RewardType);
        reward.PointCost = request.PointCost;
        reward.DiscountValue = request.DiscountValue;
        reward.ServiceId = request.ServiceId;
        reward.MinimumTier = BenefitRules.NormalizeTier(request.MinimumTier, allowAll: false);
        reward.ValidFrom = request.ValidFrom;
        reward.ValidTo = request.ValidTo;
        reward.IsActive = request.IsActive;

        await _rewardRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int rewardId)
    {
        var reward = await _rewardRepository.GetByIdAsync(rewardId);
        if (reward == null)
        {
            return false;
        }

        reward.IsActive = false;
        await _rewardRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<RewardDTO>> GetEligibleAsync(int customerId)
    {
        var customer = await GetCustomerAsync(customerId);
        var rewards = await _rewardRepository.GetActiveAsync(DateTime.UtcNow);

        return rewards
            .Where(reward => BenefitRules.IsTierEligible(customer.CurrentTier, reward.MinimumTier))
            .Select(reward => Map(reward, (customer.CurrentPoints ?? 0) >= reward.PointCost))
            .ToList();
    }

    public async Task<RewardRedemptionDTO> RedeemAsync(int customerId, int rewardId, Guid requestId)
    {
        if (requestId == Guid.Empty)
        {
            throw new ArgumentException("RequestId is required.");
        }

        var customer = await GetCustomerAsync(customerId);
        var reward = await _rewardRepository.GetByIdAsync(rewardId)
            ?? throw new KeyNotFoundException("Reward not found.");
        var now = DateTime.UtcNow;

        if (!reward.IsActive
            || (reward.ValidFrom.HasValue && reward.ValidFrom > now)
            || (reward.ValidTo.HasValue && reward.ValidTo < now))
        {
            throw new InvalidOperationException("Reward is not currently active.");
        }

        if (!BenefitRules.IsTierEligible(customer.CurrentTier, reward.MinimumTier))
        {
            throw new InvalidOperationException("Customer tier is not eligible for this reward.");
        }

        var redemption = await _rewardRepository.RedeemAsync(customerId, rewardId, requestId, now)
            ?? throw new InvalidOperationException("Insufficient points for this reward.");

        return Map(redemption);
    }

    public async Task<IReadOnlyList<RewardRedemptionDTO>> GetRedemptionsAsync(int customerId)
    {
        await GetCustomerAsync(customerId);
        return (await _rewardRepository.GetRedemptionsAsync(customerId)).Select(Map).ToList();
    }

    private async Task<Customer> GetCustomerAsync(int customerId)
    {
        return await _loyaltyRepository.GetCustomerByIdAsync(customerId)
            ?? throw new KeyNotFoundException("Customer not found.");
    }

    private async Task ValidateRequestAsync(UpsertRewardDTO request)
    {
        var type = BenefitRules.NormalizeType(request.RewardType);
        BenefitRules.NormalizeTier(request.MinimumTier, allowAll: false);
        BenefitRules.ValidateDates(request.ValidFrom, request.ValidTo);

        if (type == "Discount" && (!request.DiscountValue.HasValue || request.DiscountValue <= 0))
        {
            throw new ArgumentException("Discount reward requires a positive DiscountValue.");
        }

        if ((type == "FreeWash" || type == "AddOn") && !request.ServiceId.HasValue)
        {
            throw new ArgumentException("FreeWash and AddOn rewards require a ServiceId.");
        }

        if (request.ServiceId.HasValue
            && !await _rewardRepository.ServiceExistsAsync(request.ServiceId.Value))
        {
            throw new ArgumentException("Service not found.");
        }
    }

    private static RewardDTO Map(Reward reward, bool canRedeem)
    {
        return new RewardDTO
        {
            RewardId = reward.RewardId,
            RewardName = reward.RewardName,
            Description = reward.Description,
            RewardType = reward.RewardType,
            PointCost = reward.PointCost,
            DiscountValue = reward.DiscountValue,
            ServiceId = reward.ServiceId,
            MinimumTier = reward.MinimumTier,
            ValidFrom = reward.ValidFrom,
            ValidTo = reward.ValidTo,
            IsActive = reward.IsActive,
            CanRedeem = canRedeem,
            CreatedAt = reward.CreatedAt
        };
    }

    private static RewardRedemptionDTO Map(RewardRedemption redemption)
    {
        return new RewardRedemptionDTO
        {
            RedemptionId = redemption.RedemptionId,
            RequestId = redemption.RequestId,
            RewardId = redemption.RewardId,
            RewardName = redemption.Reward.RewardName,
            RewardType = redemption.Reward.RewardType,
            PointsSpent = redemption.PointsSpent,
            Status = redemption.Status,
            RedeemedAt = redemption.RedeemedAt,
            UsedAt = redemption.UsedAt,
            BookingId = redemption.BookingId
        };
    }
}
