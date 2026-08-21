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
            .Include(reward => reward.Service)
            .OrderByDescending(reward => reward.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Reward>> GetActiveAsync(DateTime now)
    {
        return await _context.Rewards
            .AsNoTracking()
            .Include(reward => reward.Service)
            .Where(reward => reward.IsActive
                && (!reward.ValidFrom.HasValue || reward.ValidFrom <= now)
                && (!reward.ValidTo.HasValue || reward.ValidTo >= now))
            .OrderBy(reward => reward.PointCost)
            .ToListAsync();
    }

    public Task<Reward?> GetByIdAsync(int rewardId)
    {
        return _context.Rewards
            .Include(reward => reward.Service)
            .FirstOrDefaultAsync(reward => reward.RewardId == rewardId);
    }

    public Task<Reward?> GetByIdForUpdateAsync(int rewardId)
    {
        return _context.Rewards
            .Include(reward => reward.Service)
            .FirstOrDefaultAsync(reward => reward.RewardId == rewardId);
    }

    public Task<Customer?> GetCustomerForUpdateAsync(int customerId)
    {
        return _context.Customers.FirstOrDefaultAsync(customer =>
            customer.CustomerId == customerId);
    }

    public Task<RewardRedemption?> GetRedemptionByRequestIdAsync(Guid requestId)
    {
        return _context.RewardRedemptions
            .Include(redemption => redemption.Reward)
                .ThenInclude(reward => reward.Service)
            .FirstOrDefaultAsync(redemption => redemption.RequestId == requestId);
    }

    public Task<bool> RewardNameExistsAsync(string rewardName, int? excludingRewardId = null)
    {
        return _context.Rewards.AnyAsync(reward =>
            reward.RewardName == rewardName
            && (!excludingRewardId.HasValue || reward.RewardId != excludingRewardId));
    }

    public Task<bool> ActiveServiceExistsAsync(int serviceId)
    {
        return _context.Services.AnyAsync(service =>
            service.ServiceId == serviceId
            && service.IsActive == true);
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

    public void AddRedemption(RewardRedemption redemption)
    {
        _context.RewardRedemptions.Add(redemption);
    }

    public void AddPointLedger(PointLedger transaction)
    {
        _context.PointLedgers.Add(transaction);
    }

    public async Task<T> ExecuteInSerializableTransactionAsync<T>(Func<Task<T>> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var result = await operation();
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<IReadOnlyList<RewardRedemption>> GetRedemptionsAsync(int customerId)
    {
        return await _context.RewardRedemptions
            .AsNoTracking()
            .Include(redemption => redemption.Reward)
                .ThenInclude(reward => reward.Service)
            .Where(redemption => redemption.CustomerId == customerId)
            .OrderByDescending(redemption => redemption.RedeemedAt)
            .ToListAsync();
    }

    public Task<RewardRedemption?> GetRedemptionByIdAsync(int redemptionId)
    {
        return _context.RewardRedemptions
            .AsNoTracking()
            .Include(redemption => redemption.Reward)
                .ThenInclude(reward => reward.Service)
            .FirstOrDefaultAsync(redemption => redemption.RedemptionId == redemptionId);
    }
}
