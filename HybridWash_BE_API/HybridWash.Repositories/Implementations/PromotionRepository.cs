using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations;

public class PromotionRepository : IPromotionRepository
{
    private readonly AutowashContext _context;

    public PromotionRepository(AutowashContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Promotion>> GetAllAsync()
    {
        return await _context.Promotions
            .AsNoTracking()
            .OrderByDescending(promotion => promotion.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Promotion>> GetActiveAsync(DateTime now)
    {
        return await _context.Promotions
            .AsNoTracking()
            .Where(promotion => promotion.IsActive
                && (!promotion.ValidFrom.HasValue || promotion.ValidFrom <= now)
                && (!promotion.ValidTo.HasValue || promotion.ValidTo >= now))
            .OrderBy(promotion => promotion.ValidTo)
            .ToListAsync();
    }

    public Task<Promotion?> GetByIdAsync(int promotionId)
    {
        return _context.Promotions.FirstOrDefaultAsync(promotion =>
            promotion.PromotionId == promotionId);
    }

    public Task<bool> PromoCodeExistsAsync(string promoCode, int? excludingPromotionId = null)
    {
        return _context.Promotions.AnyAsync(promotion =>
            promotion.PromoCode == promoCode
            && (!excludingPromotionId.HasValue || promotion.PromotionId != excludingPromotionId));
    }

    public Task<bool> ServiceExistsAsync(int serviceId)
    {
        return _context.Services.AnyAsync(service =>
            service.ServiceId == serviceId && service.IsActive == true);
    }

    public async Task AddAsync(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
