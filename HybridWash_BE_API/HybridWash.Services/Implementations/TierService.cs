using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Tier;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations;

public class TierService : ITierService
{
    private static readonly string[] TierNames = ["Member", "Silver", "Gold", "Platinum"];
    private readonly ITierRepository _tierRepository;

    public TierService(ITierRepository tierRepository)
    {
        _tierRepository = tierRepository;
    }

    public async Task<IReadOnlyList<TierRuleDTO>> GetRulesAsync()
    {
        return (await _tierRepository.GetRulesAsync()).Select(Map).ToList();
    }

    public async Task<TierRuleDTO?> UpdateRuleAsync(
        string tierName,
        UpdateTierRuleDTO request)
    {
        var normalizedTier = NormalizeTier(tierName);
        var qualificationMode = NormalizeQualificationMode(request.QualificationMode);
        if (normalizedTier == "Member" && !request.IsActive)
        {
            throw new ArgumentException("Member tier cannot be deactivated.");
        }

        var rule = await _tierRepository.GetRuleByNameAsync(normalizedTier, tracking: true);
        if (rule == null)
        {
            return null;
        }

        var rules = (await _tierRepository.GetRulesAsync()).ToList();
        var candidate = rules.First(item => item.TierName == normalizedTier);
        candidate.MinimumSpend = request.MinimumSpend;
        candidate.MinimumVisits = request.MinimumVisits;
        candidate.QualificationMode = qualificationMode;
        candidate.EvaluationPeriodMonths = request.EvaluationPeriodMonths;
        candidate.BookingWindowDays = request.BookingWindowDays;
        candidate.PointMultiplier = request.PointMultiplier;
        candidate.IsActive = request.IsActive;
        ValidateThresholdOrder(rules);

        rule.MinimumSpend = request.MinimumSpend;
        rule.MinimumVisits = request.MinimumVisits;
        rule.QualificationMode = qualificationMode;
        rule.EvaluationPeriodMonths = request.EvaluationPeriodMonths;
        rule.BookingWindowDays = request.BookingWindowDays;
        rule.PointMultiplier = request.PointMultiplier;
        rule.BenefitDescription = request.BenefitDescription?.Trim();
        rule.IsActive = request.IsActive;
        rule.UpdatedAt = DateTime.UtcNow;
        await _tierRepository.SaveChangesAsync();

        return Map(rule);
    }

    public async Task ReviewAfterCompletedBookingAsync(int customerId, DateTime reviewedAt)
    {
        await ReviewCustomerAsync(customerId, reviewedAt, allowDowngrade: false, "Immediate");
    }

    public async Task<TierReviewResultDTO> RunMonthlyReviewAsync(
        DateTime reviewedAt,
        bool onlyDueCustomers)
    {
        var beginningOfMonth = new DateTime(
            reviewedAt.Year,
            reviewedAt.Month,
            1,
            0,
            0,
            0,
            DateTimeKind.Utc);
        var customerIds = await _tierRepository.GetCustomerIdsForReviewAsync(
            onlyDueCustomers ? beginningOfMonth : null);
        var result = new TierReviewResultDTO();

        foreach (var customerId in customerIds)
        {
            var change = await ReviewCustomerAsync(
                customerId,
                reviewedAt,
                allowDowngrade: true,
                "Monthly");
            result.ReviewedCustomers++;
            if (change > 0)
            {
                result.UpgradedCustomers++;
            }
            else if (change < 0)
            {
                result.DowngradedCustomers++;
            }
            else
            {
                result.UnchangedCustomers++;
            }
        }

        return result;
    }

    public async Task<TierProgressDTO> GetProgressAsync(int customerId, DateTime at)
    {
        var customer = await _tierRepository.GetCustomerAsync(customerId)
            ?? throw new KeyNotFoundException("Customer not found.");
        var rules = (await _tierRepository.GetRulesAsync())
            .Where(rule => rule.IsActive)
            .OrderBy(rule => rule.Rank)
            .ToList();
        var current = FindRule(rules, customer.CurrentTier) ?? rules.First();
        var next = rules.FirstOrDefault(rule => rule.Rank > current.Rank);
        var metricRule = next ?? current;
        var metrics = await GetMetricsAsync(customerId, metricRule, at);

        return new TierProgressDTO
        {
            QualifyingSpend = metrics.Spend,
            QualifyingVisits = metrics.Visits,
            BookingWindowDays = current.BookingWindowDays,
            PointMultiplier = current.PointMultiplier,
            NextTier = next?.TierName,
            QualificationMode = metricRule.QualificationMode,
            SpendRequiredForNextTier = next == null
                ? 0
                : Math.Max(next.MinimumSpend - metrics.Spend, 0),
            VisitsRequiredForNextTier = next == null
                ? 0
                : Math.Max(next.MinimumVisits - metrics.Visits, 0)
        };
    }

    public async Task<int> GetBookingWindowDaysAsync(string? tierName)
    {
        return (await GetRuleOrMemberAsync(tierName)).BookingWindowDays;
    }

    public async Task<decimal> GetPointMultiplierAsync(string? tierName)
    {
        return (await GetRuleOrMemberAsync(tierName)).PointMultiplier;
    }

    private async Task<int> ReviewCustomerAsync(
        int customerId,
        DateTime reviewedAt,
        bool allowDowngrade,
        string reviewType)
    {
        var customer = await _tierRepository.GetCustomerAsync(customerId)
            ?? throw new KeyNotFoundException("Customer not found.");
        var rules = (await _tierRepository.GetRulesAsync())
            .Where(rule => rule.IsActive)
            .OrderByDescending(rule => rule.Rank)
            .ToList();
        var currentRule = FindRule(rules, customer.CurrentTier)
            ?? rules.OrderBy(rule => rule.Rank).First();
        var targetRule = currentRule;
        (decimal Spend, int Visits) targetMetrics = default;

        foreach (var rule in rules)
        {
            var metrics = await GetMetricsAsync(customerId, rule, reviewedAt);
            if (MeetsRule(rule, metrics))
            {
                targetRule = rule;
                targetMetrics = metrics;
                break;
            }
        }

        if (!allowDowngrade && targetRule.Rank < currentRule.Rank)
        {
            targetRule = currentRule;
            targetMetrics = await GetMetricsAsync(customerId, currentRule, reviewedAt);
        }

        var rankChange = targetRule.Rank.CompareTo(currentRule.Rank);
        if (rankChange != 0)
        {
            _tierRepository.AddHistory(new CustomerTierHistory
            {
                CustomerId = customer.CustomerId,
                PreviousTier = currentRule.TierName,
                NewTier = targetRule.TierName,
                QualifyingSpend = targetMetrics.Spend,
                QualifyingVisits = targetMetrics.Visits,
                ReviewType = reviewType,
                Reason = $"Matched {targetRule.TierName} {targetRule.QualificationMode} rule over the last {targetRule.EvaluationPeriodMonths} month(s).",
                ReviewedAt = reviewedAt
            });
            customer.CurrentTier = targetRule.TierName;
        }

        if (reviewType == "Monthly")
        {
            customer.LastTierReviewedAt = reviewedAt;
        }

        await _tierRepository.SaveChangesAsync();
        return rankChange;
    }

    private async Task<TierRule> GetRuleOrMemberAsync(string? tierName)
    {
        var rules = await _tierRepository.GetRulesAsync();
        return FindRule(rules, tierName)
            ?? FindRule(rules, "Member")
            ?? throw new InvalidOperationException("Member tier rule is missing.");
    }

    private Task<(decimal Spend, int Visits)> GetMetricsAsync(
        int customerId,
        TierRule rule,
        DateTime at)
    {
        return _tierRepository.GetQualifyingMetricsAsync(
            customerId,
            at.AddMonths(-rule.EvaluationPeriodMonths),
            at);
    }

    private static TierRule? FindRule(IEnumerable<TierRule> rules, string? tierName)
    {
        return rules.FirstOrDefault(rule =>
            rule.TierName.Equals(tierName ?? "Member", StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeTier(string tierName)
    {
        return TierNames.FirstOrDefault(name =>
            name.Equals(tierName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException("Tier must be Member, Silver, Gold or Platinum.");
    }

    private static string NormalizeQualificationMode(string? qualificationMode)
    {
        var normalized = qualificationMode?.Trim().ToUpperInvariant();
        return normalized is "AND" or "OR"
            ? normalized
            : throw new ArgumentException("QualificationMode must be AND or OR.");
    }

    private static bool MeetsRule(
        TierRule rule,
        (decimal Spend, int Visits) metrics)
    {
        var meetsSpend = metrics.Spend >= rule.MinimumSpend;
        var meetsVisits = metrics.Visits >= rule.MinimumVisits;

        return NormalizeQualificationMode(rule.QualificationMode) == "OR"
            ? meetsSpend || meetsVisits
            : meetsSpend && meetsVisits;
    }

    private static void ValidateThresholdOrder(IReadOnlyList<TierRule> rules)
    {
        var activeRules = rules.Where(rule => rule.IsActive).OrderBy(rule => rule.Rank).ToList();
        for (var index = 1; index < activeRules.Count; index++)
        {
            if (activeRules[index].MinimumSpend < activeRules[index - 1].MinimumSpend
                || activeRules[index].MinimumVisits < activeRules[index - 1].MinimumVisits)
            {
                throw new ArgumentException(
                    "Higher tiers cannot require less spend or fewer visits than lower tiers.");
            }
        }
    }

    private static TierRuleDTO Map(TierRule rule)
    {
        return new TierRuleDTO
        {
            TierRuleId = rule.TierRuleId,
            TierName = rule.TierName,
            Rank = rule.Rank,
            MinimumSpend = rule.MinimumSpend,
            MinimumVisits = rule.MinimumVisits,
            QualificationMode = rule.QualificationMode,
            EvaluationPeriodMonths = rule.EvaluationPeriodMonths,
            BookingWindowDays = rule.BookingWindowDays,
            PointMultiplier = rule.PointMultiplier,
            BenefitDescription = rule.BenefitDescription,
            IsActive = rule.IsActive,
            UpdatedAt = rule.UpdatedAt
        };
    }
}
