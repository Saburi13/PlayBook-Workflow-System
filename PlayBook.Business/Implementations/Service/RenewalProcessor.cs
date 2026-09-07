using PlayBook.Business.Interfaces.IService;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Business.Implementations.Service;

public sealed class RenewalProcessor(
    IWorkflowExecutionRepository workflowRepository,
    IWorkflowExecutionService workflowExecutionService)
{
    public async Task<int> ProcessAsync(
        DateTime now,
        IEnumerable<int> configuredOffsets,
        CancellationToken cancellationToken = default)
    {
        var offsets = configuredOffsets
            .Where(offset => offset > 0)
            .Distinct()
            .ToArray();

        var subscriptions =
            await workflowRepository.GetActiveOrExpiringSubscriptionsAsync(
                cancellationToken);

        var processed = 0;

        foreach (var subscription in subscriptions)
        {
            WorkflowExecutionService.UpdateSubscriptionStatus(
                subscription,
                now);

            foreach (var offset in offsets)
            {
                var reminderDate =
                    subscription.EndDate.AddDays(-offset);

                var reminderAlreadyExists =
                    await workflowRepository.RenewalReminderExistsAsync(
                        subscription.Id,
                        offset,
                        cancellationToken);

                if (reminderDate > now ||
                    reminderAlreadyExists)
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

                await workflowRepository.AddRenewalReminderAsync(
                    reminder,
                    cancellationToken);

                workflowRepository.AddActivity(
                    new EngagementActivity
                    {
                        Id = Guid.NewGuid(),
                        CustomerId = subscription.CustomerId,
                        SubscriptionId = subscription.Id,
                        RenewalReminderId = reminder.Id,
                        Type = "Follow-up",
                        Subject =
                            $"Plan renewal reminder ({offset} days)",
                        Description =
                            "Contact the customer about the upcoming plan expiry.",
                        ActivityDate = now
                    });

                processed++;

                await workflowExecutionService.TriggerAsync(
                    "Subscription Renewal Due",
                    "Subscription",
                    subscription.Id,
                    new
                    {
                        offsetDays = offset,
                        customerId = subscription.CustomerId
                    },
                    cancellationToken);
            }
        }

        await workflowRepository.SaveChangesAsync(
            cancellationToken);

        return processed;
    }
}