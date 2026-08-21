namespace HybridWash.Services.Implementations;

internal static class BenefitRules
{
    private static readonly string[] TierOrder = ["Member", "Silver", "Gold", "Platinum"];
    private static readonly string[] BenefitTypes = ["Discount", "FreeWash", "AddOn"];
    private static readonly string[] DiscountTypes = ["Fixed", "Percent"];

    public static string NormalizeTier(string value, bool allowAll)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Tier is required.");
        }

        var normalized = value.Trim();
        if (allowAll && normalized.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return "All";
        }

        return TierOrder.FirstOrDefault(tier => tier.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Tier must be Member, Silver, Gold, Platinum or All.");
    }

    public static string NormalizeType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Type is required.");
        }

        var normalized = value.Trim();
        return BenefitTypes.FirstOrDefault(type => type.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Type must be Discount, FreeWash or AddOn.");
    }

    public static string NormalizeDiscountType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("DiscountType is required.");
        }

        var normalized = value.Trim();
        return DiscountTypes.FirstOrDefault(type =>
            type.Equals(normalized, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("DiscountType must be Fixed or Percent.");
    }

    public static bool IsTierEligible(string? customerTier, string requiredTier)
    {
        if (requiredTier.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var customerRank = Array.FindIndex(TierOrder,
            tier => tier.Equals(customerTier ?? "Member", StringComparison.OrdinalIgnoreCase));
        var requiredRank = Array.FindIndex(TierOrder,
            tier => tier.Equals(requiredTier, StringComparison.OrdinalIgnoreCase));

        return customerRank >= 0 && requiredRank >= 0 && customerRank >= requiredRank;
    }

    public static void ValidateDates(DateTime? validFrom, DateTime? validTo)
    {
        if (validFrom.HasValue && validTo.HasValue && validFrom >= validTo)
        {
            throw new ArgumentException("ValidFrom must be earlier than ValidTo.");
        }
    }
}
