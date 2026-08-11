namespace HybridWash.Services.Implementations;

internal static class BenefitRules
{
    private static readonly string[] TierOrder = ["Member", "Silver", "Gold", "Platinum"];
    private static readonly string[] BenefitTypes = ["Discount", "FreeWash", "AddOn"];

    public static string NormalizeTier(string value, bool allowAll)
    {
        if (allowAll && value.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            return "All";
        }

        return TierOrder.FirstOrDefault(tier => tier.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Tier must be Member, Silver, Gold, Platinum or All.");
    }

    public static string NormalizeType(string value)
    {
        return BenefitTypes.FirstOrDefault(type => type.Equals(value, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Type must be Discount, FreeWash or AddOn.");
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
