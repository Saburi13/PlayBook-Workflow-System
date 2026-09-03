using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlayBook.Domain;

namespace PlayBook.Data.Context;

public class PlayBookDbContext : IdentityDbContext<IdentityUser>
{
    public PlayBookDbContext(DbContextOptions<PlayBookDbContext> options) : base(options)
    {
    }

    public DbSet<EmployeeGrade> EmployeeGrades => Set<EmployeeGrade>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalProduct> ProposalProducts => Set<ProposalProduct>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProduct> OrderProducts => Set<OrderProduct>();
    public DbSet<PlayBook.Domain.PlayBook> PlayBooks => Set<PlayBook.Domain.PlayBook>();
    public DbSet<PlayBookStep> PlayBookSteps => Set<PlayBookStep>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<Condition> Conditions => Set<Condition>();
    public DbSet<WorkflowExecution> WorkflowExecutions => Set<WorkflowExecution>();
    public DbSet<WorkflowExecutionStep> WorkflowExecutionSteps => Set<WorkflowExecutionStep>();
    public DbSet<WorkflowHistory> WorkflowHistories => Set<WorkflowHistory>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<EngagementActivity> EngagementActivities => Set<EngagementActivity>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<RenewalReminder> RenewalReminders => Set<RenewalReminder>();
    public DbSet<Voucher> Vouchers => Set<Voucher>();
    public DbSet<ProposalRevision> ProposalRevisions => Set<ProposalRevision>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.EmployeeGrade)
            .WithMany(g => g.Employees)
            .HasForeignKey(e => e.EmployeeGradeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.AssignedEmployee)
            .WithMany(e => e.Opportunities)
            .HasForeignKey(o => o.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.Opportunity)
            .WithMany(o => o.Proposals)
            .HasForeignKey(p => p.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Proposals)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.CreatedByEmployee)
            .WithMany(e => e.Proposals)
            .HasForeignKey(p => p.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Proposal)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.FromStep)
            .WithMany(s => s.SourceTransitions)
            .HasForeignKey(t => t.FromStepId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.PlayBook)
            .WithMany(p => p.Transitions)
            .HasForeignKey(t => t.PlayBookId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.ToStep)
            .WithMany(s => s.TargetTransitions)
            .HasForeignKey(t => t.ToStepId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.Condition)
            .WithMany(c => c.WorkflowTransitions)
            .HasForeignKey(t => t.ConditionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<WorkflowHistory>()
            .HasOne(h => h.WorkflowExecution)
            .WithMany(e => e.Histories)
            .HasForeignKey(h => h.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WorkflowExecution>()
            .HasOne(w => w.PlayBook)
            .WithMany(p => p.Executions)
            .HasForeignKey(w => w.PlayBookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PlayBookStep>()
            .HasOne(s => s.PlayBook)
            .WithMany(p => p.Steps)
            .HasForeignKey(s => s.PlayBookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.Proposal)
            .WithMany(p => p.Approvals)
            .HasForeignKey(a => a.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.ApproverEmployee)
            .WithMany()
            .HasForeignKey(a => a.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.WorkflowExecution)
            .WithMany()
            .HasForeignKey(a => a.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Approval>()
            .HasIndex(a => new { a.ProposalId, a.Status });

        modelBuilder.Entity<Approval>()
            .HasIndex(a => new { a.ApproverEmployeeId, a.Status });

        modelBuilder.Entity<EngagementActivity>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Activities)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Customer)
            .WithMany(c => c.Conversations)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Product)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<RenewalReminder>()
            .HasOne(r => r.Subscription)
            .WithMany(s => s.RenewalReminders)
            .HasForeignKey(r => r.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EngagementActivity>()
            .HasOne(a => a.Subscription)
            .WithMany()
            .HasForeignKey(a => a.SubscriptionId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<EngagementActivity>()
            .HasOne(a => a.RenewalReminder)
            .WithMany(r => r.Activities)
            .HasForeignKey(a => a.RenewalReminderId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<RenewalReminder>()
            .HasIndex(r => new { r.SubscriptionId, r.OffsetDays })
            .IsUnique();

        modelBuilder.Entity<Voucher>()
            .HasIndex(v => v.Code)
            .IsUnique();

        modelBuilder.Entity<ProposalRevision>()
            .HasOne(revision => revision.Proposal)
            .WithMany()
            .HasForeignKey(revision => revision.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProposalRevision>()
            .HasIndex(revision => new { revision.ProposalId, revision.Revision })
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<EmployeeGrade>()
            .Property(g => g.ApprovalLimit)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Opportunity>()
            .HasIndex(o => new { o.CustomerId, o.Status });

        modelBuilder.Entity<Opportunity>()
            .HasIndex(o => o.AssignedEmployeeId);

        modelBuilder.Entity<Proposal>()
            .HasIndex(p => new { p.OpportunityId, p.Status });

        modelBuilder.Entity<ProposalProduct>()
            .HasIndex(p => new { p.ProposalId, p.ProductId })
            .IsUnique();

        modelBuilder.Entity<OrderProduct>()
            .HasIndex(p => new { p.OrderId, p.ProductId })
            .IsUnique();

        modelBuilder.Entity<ProposalProduct>()
            .HasOne(p => p.Proposal)
            .WithMany(p => p.ProposalProducts)
            .HasForeignKey(p => p.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProposalProduct>()
            .HasOne(p => p.Product)
            .WithMany(p => p.ProposalProducts)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<OrderProduct>()
            .HasOne(p => p.Order)
            .WithMany(o => o.OrderProducts)
            .HasForeignKey(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderProduct>()
            .HasOne(p => p.Product)
            .WithMany(p => p.OrderProducts)
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.AssignedEmployee)
            .WithMany()
            .HasForeignKey(o => o.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Proposal>()
            .HasIndex(p => p.ProposalNumber)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();
    }
}
