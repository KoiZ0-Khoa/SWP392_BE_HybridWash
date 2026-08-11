using HybridWash.Repositories.Implementations;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.Implementations;
using HybridWash.Services.Interfaces;

namespace HybridWash_BE_API;

public static class LoyaltyModuleExtensions
{
    public static IServiceCollection AddLoyaltyModule(this IServiceCollection services)
    {
        services.AddScoped<ILoyaltyRepository, LoyaltyRepository>();
        services.AddScoped<ILoyaltyService, LoyaltyService>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IPromotionService, PromotionService>();
        services.AddScoped<IRewardRepository, RewardRepository>();
        services.AddScoped<IRewardService, RewardService>();

        return services;
    }
}
