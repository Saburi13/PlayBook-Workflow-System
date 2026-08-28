using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayBook.Application.Interfaces;
using PlayBook.Application.Services;
using PlayBook.Application.Workflows;
using PlayBook.Application.Approvals;
using PlayBook.Infrastructure.Approvals;
using PlayBook.Infrastructure.Data;
using PlayBook.Infrastructure.Workflows;
using PlayBook.Application.Pricing;

namespace PlayBook.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString, bool useInMemoryDatabase = false)
    {
        services.AddDbContext<PlayBookDbContext>(options =>
        {
            if (useInMemoryDatabase)
            {
                options.UseInMemoryDatabase("PlayBookDb");
                return;
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
        services.AddScoped(typeof(ICrmRepository<>), typeof(CrmRepository<>));
        services.AddScoped<IWorkflowExecutionService, WorkflowExecutionService>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddSingleton<IPricingService, PricingCalculator>();
        services.AddSingleton<VoucherService>();
        services.AddScoped<RenewalProcessor>();
        services.AddOptions<RenewalSchedulerOptions>();
        services.AddHostedService<RenewalScheduler>();

        return services;
    }
}
