using HybridWash.Repositories.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HybridWash.Services.BackgroundServices
{
    public class BookingCleanupBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingCleanupBackgroundService> _logger;

        public BookingCleanupBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<BookingCleanupBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingCleanupBackgroundService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldCancelledBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing Booking cleanup.");
                }

                // Chờ 24 giờ rồi mới quét lại (chạy 1 ngày 1 lần)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }

        private async Task CleanupOldCancelledBookingsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AutowashContext>();

            // Lấy mốc thời gian 1 tháng trước (so với ngày BookingDate)
            var oneMonthAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1));

            // Tìm các Booking trạng thái Cancelled hoặc NoShow và diễn ra từ hơn 1 tháng trước
            var oldBookings = await context.Bookings
                .Where(b => (b.Status == "Cancelled" || b.Status == "NoShow")
                         && b.BookingDate < oneMonthAgo)
                .ToListAsync(stoppingToken);

            if (oldBookings.Any())
            {
                context.Bookings.RemoveRange(oldBookings);
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("SUCCESS: Deleted {Count} old cancelled/no-show bookings from database.", oldBookings.Count);
            }
            else
            {
                _logger.LogInformation("NO ACTION: No old cancelled bookings found to delete.");
            }
        }
    }
}
