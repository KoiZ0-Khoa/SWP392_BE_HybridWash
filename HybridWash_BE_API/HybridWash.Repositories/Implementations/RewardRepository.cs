using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace HybridWash.Repositories.Implementations;

public class RewardRepository : IRewardRepository
{
    private readonly AutowashContext _context;

    public RewardRepository(AutowashContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Reward>> GetAllAsync()
    {
        return await _context.Rewards
            .AsNoTracking()
            .OrderByDescending(reward => reward.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Reward>> GetActiveAsync(DateTime now)
    {
        return await _context.Rewards
            .AsNoTracking()
            .Where(reward => reward.IsActive
                && (!reward.ValidFrom.HasValue || reward.ValidFrom <= now)
                && (!reward.ValidTo.HasValue || reward.ValidTo >= now))
            .OrderBy(reward => reward.PointCost)
            .ToListAsync();
    }

    public Task<Reward?> GetByIdAsync(int rewardId)
    {
        return _context.Rewards.FirstOrDefaultAsync(reward => reward.RewardId == rewardId);
    }

    public Task<bool> RewardNameExistsAsync(string rewardName, int? excludingRewardId = null)
    {
        return _context.Rewards.AnyAsync(reward =>
            reward.RewardName == rewardName
            && (!excludingRewardId.HasValue || reward.RewardId != excludingRewardId));
    }

    public Task<bool> ServiceExistsAsync(int serviceId)
    {
        return _context.Services.AnyAsync(service => service.ServiceId == serviceId);
    }

    public async Task AddAsync(Reward reward)
    {
        _context.Rewards.Add(reward);
        await _context.SaveChangesAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public async Task<RewardRedemption?> RedeemAsync(
        int customerId,
        int rewardId,
        Guid requestId,
        DateTime redeemedAt)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var existing = await _context.RewardRedemptions
            .Include(redemption => redemption.Reward)
            .FirstOrDefaultAsync(redemption => redemption.RequestId == requestId);
        if (existing != null)
        {
            await transaction.CommitAsync();
            return existing.CustomerId == customerId ? existing : null;
        }

        var reward = await _context.Rewards.FirstAsync(item => item.RewardId == rewardId);
        var customer = await _context.Customers.FirstAsync(item => item.CustomerId == customerId);
        if ((customer.CurrentPoints ?? 0) < reward.PointCost)
        {
            await transaction.RollbackAsync();
            return null;
        }

        customer.CurrentPoints = (customer.CurrentPoints ?? 0) - reward.PointCost;

        var redemption = new RewardRedemption
        {
            RequestId = requestId,
            CustomerId = customerId,
            RewardId = rewardId,
            PointsSpent = reward.PointCost,
            Status = "Issued",
            RedeemedAt = redeemedAt,
            Reward = reward
        };

        _context.RewardRedemptions.Add(redemption);
        await _context.SaveChangesAsync();

        _context.PointLedgers.Add(new PointLedger
        {
            CustomerId = customerId,
            RewardRedemptionId = redemption.RedemptionId,
            Points = -reward.PointCost,
            TransactionType = "Redeem",
            Description = $"Redeemed reward: {reward.RewardName}",
            CreatedAt = redeemedAt
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return redemption;
    }

    public async Task<IReadOnlyList<RewardRedemption>> GetRedemptionsAsync(int customerId)
    {
        return await _context.RewardRedemptions
            .AsNoTracking()
            .Include(redemption => redemption.Reward)
            .Where(redemption => redemption.CustomerId == customerId)
            .OrderByDescending(redemption => redemption.RedeemedAt)
            .ToListAsync();
    }
}
