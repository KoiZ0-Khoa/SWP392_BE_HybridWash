using HybridWash.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HybridWash.Services.BackgroundServices;

public class MonthlyTierReviewService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MonthlyTierReviewService> _logger;

    public MonthlyTierReviewService(
        IServiceProvider serviceProvider,
        ILogger<MonthlyTierReviewService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tierService = scope.ServiceProvider.GetRequiredService<ITierService>();
                var result = await tierService.RunMonthlyReviewAsync(
                    DateTime.UtcNow,
                    onlyDueCustomers: true);

                if (result.ReviewedCustomers > 0)
                {
                    _logger.LogInformation(
                        "Monthly tier review processed {Count} customers: {Upgraded} upgraded, {Downgraded} downgraded.",
                        result.ReviewedCustomers,
                        result.UpgradedCustomers,
                        result.DowngradedCustomers);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Monthly tier review failed.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
