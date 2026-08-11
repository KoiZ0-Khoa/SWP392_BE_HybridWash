using HybridWash.Repositories.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HybridWash.Services.BackgroundServices
{
    public class WashStatusUpdaterService : BackgroundService
    {
        private readonly ILogger<WashStatusUpdaterService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WashStatusUpdaterService(ILogger<WashStatusUpdaterService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("WashStatusUpdaterService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateWashingStatusesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating wash statuses.");
                }

                // Chạy lặp lại mỗi 1 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("WashStatusUpdaterService is stopping.");
        }

        private async Task UpdateWashingStatusesAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AutowashContext>();

                // Tìm các xe đang rửa (Washing)
                var washingBookings = await dbContext.Bookings
                    .Where(b => b.Status == "Washing" && b.ActualWashTime.HasValue)
                    .ToListAsync(stoppingToken);

                var now = DateTime.UtcNow;
                var updatedCount = 0;

                foreach (var booking in washingBookings)
                {
                    // Nếu thời gian bắt đầu rửa + 30 phút <= hiện tại -> Đổi sang Completed
                    var washEndTime = booking.ActualWashTime!.Value.AddMinutes(30);
                    
                    if (now >= washEndTime)
                    {
                        booking.Status = "Completed";
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    await dbContext.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation($"Updated {updatedCount} bookings from 'Washing' to 'Completed'.");
                }
            }
        }
    }
}
