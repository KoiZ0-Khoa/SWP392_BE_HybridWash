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
        if (string.IsNullOrWhiteSpace(request.RewardName))
        {
            throw new ArgumentException("RewardName is required.");
        }

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
        var createdReward = await _rewardRepository.GetByIdAsync(reward.RewardId)
            ?? throw new InvalidOperationException("Created reward could not be reloaded.");
        return Map(createdReward, false);
    }

    public async Task<bool> UpdateAsync(int rewardId, UpsertRewardDTO request)
    {
        var reward = await _rewardRepository.GetByIdAsync(rewardId);
        if (reward == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.RewardName))
        {
            throw new ArgumentException("RewardName is required.");
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

        if (rewardId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rewardId));
        }

        return await _rewardRepository.ExecuteInSerializableTransactionAsync(async () =>
        {
            var existing = await _rewardRepository.GetRedemptionByRequestIdAsync(requestId);
            if (existing != null)
            {
                if (existing.CustomerId != customerId || existing.RewardId != rewardId)
                {
                    throw new InvalidOperationException(
                        "RequestId has already been used for another reward redemption.");
                }

                return Map(existing);
            }

            var customer = await _rewardRepository.GetCustomerForUpdateAsync(customerId)
                ?? throw new KeyNotFoundException("Customer not found.");
            var reward = await _rewardRepository.GetByIdForUpdateAsync(rewardId)
                ?? throw new KeyNotFoundException("Reward not found.");
            var now = DateTime.UtcNow;

            ValidateRewardAvailability(reward, customer, now);
            await ValidateStoredRewardConfigurationAsync(reward);

            if ((customer.CurrentPoints ?? 0) < reward.PointCost)
            {
                throw new InvalidOperationException("Insufficient points for this reward.");
            }

            customer.CurrentPoints = (customer.CurrentPoints ?? 0) - reward.PointCost;
            var redemption = new RewardRedemption
            {
                RequestId = requestId,
                CustomerId = customerId,
                RewardId = rewardId,
                PointsSpent = reward.PointCost,
                Status = "Issued",
                RedeemedAt = now,
                Reward = reward
            };

            _rewardRepository.AddRedemption(redemption);
            await _rewardRepository.SaveChangesAsync();

            _rewardRepository.AddPointLedger(new PointLedger
            {
                CustomerId = customerId,
                RewardRedemptionId = redemption.RedemptionId,
                Points = -reward.PointCost,
                TransactionType = "Redeem",
                Description = $"Redeemed reward: {reward.RewardName}",
                CreatedAt = now
            });

            await _rewardRepository.SaveChangesAsync();
            return Map(redemption);
        });
    }

    private static void ValidateRewardAvailability(
        Reward reward,
        Customer customer,
        DateTime now)
    {
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
    }

    private async Task ValidateStoredRewardConfigurationAsync(Reward reward)
    {
        if (reward.PointCost <= 0)
        {
            throw new InvalidOperationException("Reward point cost is invalid.");
        }

        var type = BenefitRules.NormalizeType(reward.RewardType);
        if (type == "Discount")
        {
            if (!reward.DiscountValue.HasValue || reward.DiscountValue <= 0)
            {
                throw new InvalidOperationException(
                    "Discount reward configuration is incomplete.");
            }
        }
        else if (!reward.ServiceId.HasValue)
        {
            throw new InvalidOperationException(
                $"{type} reward configuration is incomplete.");
        }

        await ValidateRewardServiceAsync(reward.ServiceId);
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

        if (type == "Discount")
        {
            await ValidateRewardServiceAsync(request.ServiceId);
            return;
        }

        if (!request.ServiceId.HasValue)
        {
            throw new ArgumentException("FreeWash and AddOn rewards require a ServiceId.");
        }

        if (request.DiscountValue.HasValue)
        {
            throw new ArgumentException(
                "DiscountValue can only be used for Discount rewards.");
        }

        await ValidateRewardServiceAsync(request.ServiceId);
    }

    private async Task ValidateRewardServiceAsync(int? serviceId)
    {
        if (serviceId.HasValue
            && !await _rewardRepository.ActiveServiceExistsAsync(serviceId.Value))
        {
            throw new ArgumentException("Service not found or inactive.");
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
            ServiceName = reward.Service?.ServiceName,
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
            Description = redemption.Reward.Description,
            DiscountValue = redemption.Reward.DiscountValue,
            ServiceId = redemption.Reward.ServiceId,
            ServiceName = redemption.Reward.Service?.ServiceName,
            PointsSpent = redemption.PointsSpent,
            Status = redemption.Status,
            RedeemedAt = redemption.RedeemedAt,
            UsedAt = redemption.UsedAt,
            BookingId = redemption.BookingId
        };
    }
}
