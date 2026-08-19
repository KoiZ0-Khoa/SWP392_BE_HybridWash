using HybridWash.Repositories.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HybridWash.Services.BackgroundServices
{
    public class BookingStatusAutoUpdateService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BookingStatusAutoUpdateService> _logger;

        public BookingStatusAutoUpdateService(
            IServiceProvider serviceProvider,
            ILogger<BookingStatusAutoUpdateService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BookingStatusAutoUpdateService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MarkNoShowBookingsAsync(stoppingToken);
                    await CleanupExpiredPendingBookingsAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in BookingStatusAutoUpdateService.");
                }

                // Quét mỗi 5 phút
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task MarkNoShowBookingsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AutowashContext>();

            var now = DateTime.UtcNow.AddHours(7);
            var today = DateOnly.FromDateTime(now);
            var currentTime = TimeOnly.FromDateTime(now);

            var noShowBookings = await context.Bookings
                .Include(b => b.Slot)
                .Where(b => (b.Status == "Deposited" || b.Status == "Confirmed")
                         && b.BookingDate <= today
                         && (b.BookingDate < today || b.Slot.EndTime <= currentTime))
                .ToListAsync(stoppingToken);

            if (noShowBookings.Any())
            {
                foreach (var booking in noShowBookings)
                {
                    booking.Status = "NoShow";
                }

                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Marked {Count} booking(s) as NoShow.", noShowBookings.Count);
            }
        }

        private async Task CleanupExpiredPendingBookingsAsync(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AutowashContext>();

            var tenMinutesAgo = DateTime.UtcNow.AddMinutes(-1);

            var expiredPendingBookings = await context.Bookings
                .Include(b => b.RewardRedemptions)
                .Include(b => b.BookingAddOns)
                .Where(b => b.Status == "Pending"
                         && b.CreatedAt != null
                         && b.CreatedAt <= tenMinutesAgo)
                .ToListAsync(stoppingToken);

            if (expiredPendingBookings.Any())
            {
                foreach (var booking in expiredPendingBookings)
                {
                    foreach (var redemption in booking.RewardRedemptions)
                    {
                        redemption.BookingId = null;
                        redemption.Status = "Issued";
                        redemption.UsedAt = null;
                    }

                    context.BookingAddOns.RemoveRange(booking.BookingAddOns);
                }

                context.Bookings.RemoveRange(expiredPendingBookings);
                await context.SaveChangesAsync(stoppingToken);
                _logger.LogInformation("Deleted {Count} expired Pending booking(s).", expiredPendingBookings.Count);
            }
        }
    }
}
