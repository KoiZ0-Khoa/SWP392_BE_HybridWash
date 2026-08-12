using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces;

public interface IPromotionRepository
{
    Task<IReadOnlyList<Promotion>> GetAllAsync();
    Task<IReadOnlyList<Promotion>> GetActiveAsync(DateTime now);
    Task<Promotion?> GetByIdAsync(int promotionId);
    Task<bool> PromoCodeExistsAsync(string promoCode, int? excludingPromotionId = null);
    Task<bool> ServiceExistsAsync(int serviceId);
    Task AddAsync(Promotion promotion);
    Task SaveChangesAsync();
}
