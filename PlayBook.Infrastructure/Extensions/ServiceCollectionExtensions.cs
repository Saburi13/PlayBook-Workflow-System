using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PlayBook.Data.Context;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Data.Repositories.Implementations;

using PlayBook.Business.Services.Interfaces;
using PlayBook.Business.Services.Implementations;

using PlayBook.Infrastructure.Workflows;

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
        services.AddScoped<IApprovalRepository, ApprovalRepository>();
        services.AddScoped<IApprovalService, ApprovalService>();
        services.AddSingleton<IPricingService, PricingCalculator>();
        services.AddSingleton<VoucherService>();
        services.AddScoped<RenewalProcessor>();
        services.AddOptions<RenewalSchedulerOptions>();
        services.AddHostedService<RenewalScheduler>();

        return services;
    }
}
