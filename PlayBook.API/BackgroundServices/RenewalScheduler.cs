using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayBook.Business.Implementations.Service;

namespace PlayBook.API.BackgroundServices;

public sealed class RenewalSchedulerOptions
{
    public int[] ReminderOffsetsDays { get; set; } = [90, 60, 30];

    public TimeSpan PollInterval { get; set; } =
        TimeSpan.FromHours(1);
}

public sealed class RenewalScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<RenewalSchedulerOptions> options,
    ILogger<RenewalScheduler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    scopeFactory.CreateScope();

                var processor =
                    scope.ServiceProvider
                        .GetRequiredService<RenewalProcessor>();

                await processor.ProcessAsync(
                    DateTime.UtcNow,
                    options.Value.ReminderOffsetsDays,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Renewal processing failed.");
            }

            await Task.Delay(
                options.Value.PollInterval,
                stoppingToken);
        }
    }
}