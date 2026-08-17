using HybridWash.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HybridWash.Services.BackgroundServices;

public class PointExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PointExpiryBackgroundService> _logger;

    public PointExpiryBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<PointExpiryBackgroundService> logger)
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
                var loyaltyService = scope.ServiceProvider.GetRequiredService<ILoyaltyService>();
                var result = await loyaltyService.ExpirePointsAsync(DateTime.UtcNow);

                if (result.ProcessedEarnTransactions > 0)
                {
                    _logger.LogInformation(
                        "Point expiry processed {Transactions} earn transactions for {Customers} customers; {Points} points expired.",
                        result.ProcessedEarnTransactions,
                        result.ProcessedCustomers,
                        result.ExpiredPoints);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Point expiry processing failed.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
