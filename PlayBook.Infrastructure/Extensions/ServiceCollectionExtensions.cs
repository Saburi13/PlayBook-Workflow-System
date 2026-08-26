using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PlayBook.Application.Interfaces;
using PlayBook.Application.Services;
using PlayBook.Application.Workflows;
using PlayBook.Application.Approvals;
using PlayBook.Infrastructure.Approvals;
using PlayBook.Infrastructure.Data;
using PlayBook.Infrastructure.Workflows;

namespace PlayBook.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PlayBookDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IConditionEvaluator, ConditionEvaluator>();
        services.AddScoped(typeof(ICrmRepository<>), typeof(CrmRepository<>));
        services.AddScoped<IWorkflowExecutionService, WorkflowExecutionService>();
        services.AddScoped<IApprovalService, ApprovalService>();

        return services;
    }
}
