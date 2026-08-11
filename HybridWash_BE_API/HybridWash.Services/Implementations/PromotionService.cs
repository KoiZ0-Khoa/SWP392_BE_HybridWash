using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Promotion;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepository;
    private readonly ILoyaltyRepository _loyaltyRepository;

    public PromotionService(
        IPromotionRepository promotionRepository,
        ILoyaltyRepository loyaltyRepository)
    {
        _promotionRepository = promotionRepository;
        _loyaltyRepository = loyaltyRepository;
    }

    public async Task<IReadOnlyList<PromotionDTO>> GetAllAsync()
    {
        return (await _promotionRepository.GetAllAsync()).Select(Map).ToList();
    }

    public async Task<IReadOnlyList<PromotionDTO>> GetPublicAsync()
    {
        return (await _promotionRepository.GetActiveAsync(DateTime.UtcNow)).Select(Map).ToList();
    }

    public async Task<PromotionDTO?> GetByIdAsync(int promotionId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        return promotion == null ? null : Map(promotion);
    }

    public async Task<PromotionDTO> CreateAsync(UpsertPromotionDTO request)
    {
        var normalizedCode = NormalizeCode(request.PromoCode);
        if (normalizedCode != null && await _promotionRepository.PromoCodeExistsAsync(normalizedCode))
        {
            throw new InvalidOperationException("Promotion code already exists.");
        }

        var promotion = new Promotion
        {
            PromoCode = normalizedCode,
            PromoName = request.PromoName.Trim(),
            Description = request.Description?.Trim(),
            PromoType = BenefitRules.NormalizeType(request.PromoType),
            TargetTier = BenefitRules.NormalizeTier(request.TargetTier, allowAll: true),
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        BenefitRules.ValidateDates(promotion.ValidFrom, promotion.ValidTo);
        await _promotionRepository.AddAsync(promotion);
        return Map(promotion);
    }

    public async Task<bool> UpdateAsync(int promotionId, UpsertPromotionDTO request)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        if (promotion == null)
        {
            return false;
        }

        var normalizedCode = NormalizeCode(request.PromoCode);
        if (normalizedCode != null
            && await _promotionRepository.PromoCodeExistsAsync(normalizedCode, promotionId))
        {
            throw new InvalidOperationException("Promotion code already exists.");
        }

        BenefitRules.ValidateDates(request.ValidFrom, request.ValidTo);

        promotion.PromoCode = normalizedCode;
        promotion.PromoName = request.PromoName.Trim();
        promotion.Description = request.Description?.Trim();
        promotion.PromoType = BenefitRules.NormalizeType(request.PromoType);
        promotion.TargetTier = BenefitRules.NormalizeTier(request.TargetTier, allowAll: true);
        promotion.ValidFrom = request.ValidFrom;
        promotion.ValidTo = request.ValidTo;
        promotion.IsActive = request.IsActive;

        await _promotionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeactivateAsync(int promotionId)
    {
        var promotion = await _promotionRepository.GetByIdAsync(promotionId);
        if (promotion == null)
        {
            return false;
        }

        promotion.IsActive = false;
        await _promotionRepository.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<PromotionDTO>> GetEligibleAsync(int customerId)
    {
        var customer = await _loyaltyRepository.GetCustomerByIdAsync(customerId)
            ?? throw new KeyNotFoundException("Customer not found.");
        var promotions = await _promotionRepository.GetActiveAsync(DateTime.UtcNow);

        return promotions
            .Where(promotion => BenefitRules.IsTierEligible(
                customer.CurrentTier,
                promotion.TargetTier ?? "All"))
            .Select(Map)
            .ToList();
    }

    private static string? NormalizeCode(string? promoCode)
    {
        return string.IsNullOrWhiteSpace(promoCode) ? null : promoCode.Trim().ToUpperInvariant();
    }

    private static PromotionDTO Map(Promotion promotion)
    {
        return new PromotionDTO
        {
            PromotionId = promotion.PromotionId,
            PromoCode = promotion.PromoCode,
            PromoName = promotion.PromoName,
            Description = promotion.Description,
            PromoType = promotion.PromoType ?? string.Empty,
            TargetTier = promotion.TargetTier ?? "All",
            ValidFrom = promotion.ValidFrom,
            ValidTo = promotion.ValidTo,
            IsActive = promotion.IsActive,
            CreatedAt = promotion.CreatedAt
        };
    }
}
