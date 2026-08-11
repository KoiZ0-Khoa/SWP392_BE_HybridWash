using HybridWash.Services.DTOs.Promotion;

namespace HybridWash.Services.Interfaces;

public interface IPromotionService
{
    Task<IReadOnlyList<PromotionDTO>> GetAllAsync();
    Task<PromotionDTO?> GetByIdAsync(int promotionId);
    Task<PromotionDTO> CreateAsync(UpsertPromotionDTO request);
    Task<bool> UpdateAsync(int promotionId, UpsertPromotionDTO request);
    Task<bool> DeactivateAsync(int promotionId);
    Task<IReadOnlyList<PromotionDTO>> GetEligibleAsync(int customerId);
}
