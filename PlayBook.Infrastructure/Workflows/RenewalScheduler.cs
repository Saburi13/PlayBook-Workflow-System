using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlayBook.Application.Workflows;
using PlayBook.Domain;
using PlayBook.Infrastructure.Data;

namespace PlayBook.Infrastructure.Workflows;

public sealed class RenewalSchedulerOptions
{
    public int[] ReminderOffsetsDays { get; set; } = [90, 60, 30];
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromHours(1);
}

public sealed class RenewalProcessor(PlayBookDbContext dbContext, IWorkflowExecutionService workflowExecutionService)
{
    public async Task<int> ProcessAsync(DateTime now, IEnumerable<int> configuredOffsets, CancellationToken cancellationToken = default)
    {
        var offsets = configuredOffsets.Where(offset => offset > 0).Distinct().ToArray();
        var subscriptions = await dbContext.Subscriptions
            .Include(subscription => subscription.Product)
            .Where(subscription => subscription.Status == SubscriptionStatus.Active || subscription.Status == SubscriptionStatus.Expiring)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var subscription in subscriptions)
        {
            WorkflowExecutionService.UpdateSubscriptionStatus(subscription, now);
            foreach (var offset in offsets)
            {
                var reminderDate = subscription.EndDate.AddDays(-offset);
                if (reminderDate > now || await dbContext.RenewalReminders.AnyAsync(reminder => reminder.SubscriptionId == subscription.Id && reminder.OffsetDays == offset, cancellationToken))
                {
                    continue;
                }

                var reminder = new RenewalReminder
                {
                    Id = Guid.NewGuid(),
                    SubscriptionId = subscription.Id,
                    OffsetDays = offset,
                    ReminderDate = reminderDate,
                    ProcessedAt = now
                };
                dbContext.RenewalReminders.Add(reminder);
                dbContext.EngagementActivities.Add(new EngagementActivity
                {
                    Id = Guid.NewGuid(),
                    CustomerId = subscription.CustomerId,
                    SubscriptionId = subscription.Id,
                    RenewalReminderId = reminder.Id,
                    Type = "Follow-up",
                    Subject = $"Plan renewal reminder ({offset} days)",
                    Description = "Contact the customer about the upcoming plan expiry.",
                    ActivityDate = now
                });
                processed++;
                await workflowExecutionService.TriggerAsync("Subscription Renewal Due", "Subscription", subscription.Id, new { offsetDays = offset, customerId = subscription.CustomerId }, cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return processed;
    }
}

public sealed class RenewalScheduler(
    IServiceScopeFactory scopeFactory,
    IOptions<RenewalSchedulerOptions> options,
    ILogger<RenewalScheduler> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<RenewalProcessor>();
                await processor.ProcessAsync(DateTime.UtcNow, options.Value.ReminderOffsetsDays, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Renewal processing failed.");
            }

            await Task.Delay(options.Value.PollInterval, stoppingToken);
        }
    }
}
