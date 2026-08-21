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

        var benefit = await ValidateBenefitAsync(request);
        var promotion = new Promotion
        {
            PromoCode = normalizedCode,
            PromoName = request.PromoName.Trim(),
            Description = request.Description?.Trim(),
            PromoType = benefit.PromoType,
            DiscountType = benefit.DiscountType,
            DiscountValue = benefit.DiscountValue,
            MaxDiscount = benefit.MaxDiscount,
            ServiceId = request.ServiceId,
            TargetTier = BenefitRules.NormalizeTier(request.TargetTier, allowAll: true),
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

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

        var benefit = await ValidateBenefitAsync(request);

        promotion.PromoCode = normalizedCode;
        promotion.PromoName = request.PromoName.Trim();
        promotion.Description = request.Description?.Trim();
        promotion.PromoType = benefit.PromoType;
        promotion.DiscountType = benefit.DiscountType;
        promotion.DiscountValue = benefit.DiscountValue;
        promotion.MaxDiscount = benefit.MaxDiscount;
        promotion.ServiceId = request.ServiceId;
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

    private async Task<PromotionBenefit> ValidateBenefitAsync(UpsertPromotionDTO request)
    {
        if (string.IsNullOrWhiteSpace(request.PromoName))
        {
            throw new ArgumentException("PromoName is required.");
        }

        BenefitRules.ValidateDates(request.ValidFrom, request.ValidTo);
        var promoType = BenefitRules.NormalizeType(request.PromoType);

        if (request.ServiceId.HasValue
            && !await _promotionRepository.ServiceExistsAsync(request.ServiceId.Value))
        {
            throw new KeyNotFoundException("Service not found or inactive.");
        }

        if (promoType == "Discount")
        {
            if (string.IsNullOrWhiteSpace(request.DiscountType))
            {
                throw new ArgumentException("DiscountType is required for Discount promotion.");
            }

            var discountType = BenefitRules.NormalizeDiscountType(request.DiscountType);
            if (!request.DiscountValue.HasValue || request.DiscountValue <= 0)
            {
                throw new ArgumentException("DiscountValue must be greater than 0.");
            }

            if (discountType == "Percent" && request.DiscountValue > 100)
            {
                throw new ArgumentException("Percent DiscountValue cannot exceed 100.");
            }

            if (discountType == "Percent"
                && request.MaxDiscount.HasValue
                && request.MaxDiscount <= 0)
            {
                throw new ArgumentException("MaxDiscount must be greater than 0 when provided.");
            }

            if (discountType == "Fixed" && request.MaxDiscount.HasValue)
            {
                throw new ArgumentException("MaxDiscount is only used for Percent discount.");
            }

            return new PromotionBenefit(
                promoType,
                discountType,
                request.DiscountValue,
                discountType == "Percent" ? request.MaxDiscount : null);
        }

        if (!request.ServiceId.HasValue)
        {
            throw new ArgumentException($"ServiceId is required for {promoType} promotion.");
        }

        if (!string.IsNullOrWhiteSpace(request.DiscountType)
            || request.DiscountValue.HasValue
            || request.MaxDiscount.HasValue)
        {
            throw new ArgumentException(
                "Discount fields can only be used for Discount promotion.");
        }

        return new PromotionBenefit(promoType, null, null, null);
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
            DiscountType = promotion.DiscountType,
            DiscountValue = promotion.DiscountValue,
            MaxDiscount = promotion.MaxDiscount,
            ServiceId = promotion.ServiceId,
            TargetTier = promotion.TargetTier ?? "All",
            ValidFrom = promotion.ValidFrom,
            ValidTo = promotion.ValidTo,
            IsActive = promotion.IsActive,
            CreatedAt = promotion.CreatedAt
        };
    }

    private sealed record PromotionBenefit(
        string PromoType,
        string? DiscountType,
        decimal? DiscountValue,
        decimal? MaxDiscount);
}
