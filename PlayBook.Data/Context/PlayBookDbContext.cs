using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlayBook.Domain;

namespace PlayBook.Data.Context;

public class PlayBookDbContext : IdentityDbContext<IdentityUser>
{
    public PlayBookDbContext(DbContextOptions<PlayBookDbContext> options)
        : base(options)
    {
    }

    // =========================================================
    // Existing PlayBook DbSets
    // =========================================================

    public DbSet<EmployeeGrade> EmployeeGrades => Set<EmployeeGrade>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Proposal> Proposals => Set<Proposal>();
    public DbSet<ProposalProduct> ProposalProducts => Set<ProposalProduct>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProduct> OrderProducts => Set<OrderProduct>();

    public DbSet<PlayBook.Domain.PlayBook> PlayBooks =>
        Set<PlayBook.Domain.PlayBook>();

    public DbSet<PlayBookStep> PlayBookSteps =>
        Set<PlayBookStep>();

    public DbSet<WorkflowTransition> WorkflowTransitions =>
        Set<WorkflowTransition>();

    public DbSet<Condition> Conditions =>
        Set<Condition>();

    public DbSet<WorkflowExecution> WorkflowExecutions =>
        Set<WorkflowExecution>();

    public DbSet<WorkflowExecutionStep> WorkflowExecutionSteps =>
        Set<WorkflowExecutionStep>();

    public DbSet<WorkflowHistory> WorkflowHistories =>
        Set<WorkflowHistory>();

    public DbSet<Approval> Approvals =>
        Set<Approval>();

    public DbSet<EngagementActivity> EngagementActivities =>
        Set<EngagementActivity>();

    public DbSet<Conversation> Conversations =>
        Set<Conversation>();

    public DbSet<Subscription> Subscriptions =>
        Set<Subscription>();

    public DbSet<RenewalReminder> RenewalReminders =>
        Set<RenewalReminder>();

    public DbSet<Voucher> Vouchers =>
        Set<Voucher>();

    public DbSet<ProposalRevision> ProposalRevisions =>
        Set<ProposalRevision>();

    // =========================================================
    // AMS DbSets
    // =========================================================

    public DbSet<Account> Accounts =>
        Set<Account>();

    public DbSet<AccountContacts> AccountContacts =>
        Set<AccountContacts>();

    public DbSet<AccountAddress> AccountAddresses =>
        Set<AccountAddress>();

    public DbSet<AccountAddressType> AccountAddressTypes =>
        Set<AccountAddressType>();

    // =========================================================
    // Model Configuration
    // =========================================================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================
        // Employee -> Manager
        // =====================================================

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Manager)
            .WithMany(e => e.DirectReports)
            .HasForeignKey(e => e.ManagerId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Employee -> EmployeeGrade
        // =====================================================

        modelBuilder.Entity<Employee>()
            .HasOne(e => e.EmployeeGrade)
            .WithMany(g => g.Employees)
            .HasForeignKey(e => e.EmployeeGradeId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Opportunity -> Customer
        // =====================================================

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Opportunities)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Opportunity -> Assigned Employee
        // =====================================================

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.AssignedEmployee)
            .WithMany(e => e.Opportunities)
            .HasForeignKey(o => o.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Opportunity -> Account
        // =====================================================

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.Account)
            .WithMany()
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Opportunity -> Account Contact
        // =====================================================

        modelBuilder.Entity<Opportunity>()
            .HasOne(o => o.ContactPerson)
            .WithMany()
            .HasForeignKey(o => o.ContactPersonId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Proposal -> Opportunity
        // =====================================================

        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.Opportunity)
            .WithMany(o => o.Proposals)
            .HasForeignKey(p => p.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Proposal -> Customer
        // =====================================================

        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.Customer)
            .WithMany(c => c.Proposals)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Proposal -> Created By Employee
        // =====================================================

        modelBuilder.Entity<Proposal>()
            .HasOne(p => p.CreatedByEmployee)
            .WithMany(e => e.Proposals)
            .HasForeignKey(p => p.CreatedByEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Order -> Proposal
        // =====================================================

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Proposal)
            .WithMany(p => p.Orders)
            .HasForeignKey(o => o.ProposalId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Order -> Customer
        // =====================================================

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Order -> Assigned Employee
        // =====================================================

        modelBuilder.Entity<Order>()
            .HasOne(o => o.AssignedEmployee)
            .WithMany()
            .HasForeignKey(o => o.AssignedEmployeeId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Order -> Opportunity
        // =====================================================

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Opportunity)
            .WithMany(o => o.Orders)
            .HasForeignKey(o => o.OpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Order -> Account
        // =====================================================

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Account)
            .WithMany()
            .HasForeignKey(o => o.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Order -> Account Contact
        // =====================================================

        modelBuilder.Entity<Order>()
            .HasOne(o => o.ContactPerson)
            .WithMany()
            .HasForeignKey(o => o.ContactPersonId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // WorkflowTransition -> FromStep
        // =====================================================

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.FromStep)
            .WithMany(s => s.SourceTransitions)
            .HasForeignKey(t => t.FromStepId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // WorkflowTransition -> PlayBook
        // =====================================================

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.PlayBook)
            .WithMany(p => p.Transitions)
            .HasForeignKey(t => t.PlayBookId)
            .OnDelete(DeleteBehavior.NoAction);

        // =====================================================
        // WorkflowTransition -> ToStep
        // =====================================================

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.ToStep)
            .WithMany(s => s.TargetTransitions)
            .HasForeignKey(t => t.ToStepId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // WorkflowTransition -> Condition
        // =====================================================

        modelBuilder.Entity<WorkflowTransition>()
            .HasOne(t => t.Condition)
            .WithMany(c => c.WorkflowTransitions)
            .HasForeignKey(t => t.ConditionId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // WorkflowHistory -> WorkflowExecution
        // =====================================================

        modelBuilder.Entity<WorkflowHistory>()
            .HasOne(h => h.WorkflowExecution)
            .WithMany(e => e.Histories)
            .HasForeignKey(h => h.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // WorkflowExecution -> PlayBook
        // =====================================================

        modelBuilder.Entity<WorkflowExecution>()
            .HasOne(w => w.PlayBook)
            .WithMany(p => p.Executions)
            .HasForeignKey(w => w.PlayBookId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // PlayBookStep -> PlayBook
        // =====================================================

        modelBuilder.Entity<PlayBookStep>()
            .HasOne(s => s.PlayBook)
            .WithMany(p => p.Steps)
            .HasForeignKey(s => s.PlayBookId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Approval -> Proposal
        // =====================================================

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.Proposal)
            .WithMany(p => p.Approvals)
            .HasForeignKey(a => a.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Approval -> Approver Employee
        // =====================================================

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.ApproverEmployee)
            .WithMany()
            .HasForeignKey(a => a.ApproverEmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Approval -> WorkflowExecution
        // =====================================================

        modelBuilder.Entity<Approval>()
            .HasOne(a => a.WorkflowExecution)
            .WithMany()
            .HasForeignKey(a => a.WorkflowExecutionId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Approval indexes
        // =====================================================

        modelBuilder.Entity<Approval>()
            .HasIndex(a => new
            {
                a.ProposalId,
                a.Status
            });

        modelBuilder.Entity<Approval>()
            .HasIndex(a => new
            {
                a.ApproverEmployeeId,
                a.Status
            });

        // =====================================================
        // EngagementActivity -> Customer
        // =====================================================

        modelBuilder.Entity<EngagementActivity>()
            .HasOne(a => a.Customer)
            .WithMany(c => c.Activities)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Conversation -> Customer
        // =====================================================

        modelBuilder.Entity<Conversation>()
            .HasOne(c => c.Customer)
            .WithMany(c => c.Conversations)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Subscription -> Customer
        // =====================================================

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // Subscription -> Product
        // =====================================================

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Product)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // RenewalReminder -> Subscription
        // =====================================================

        modelBuilder.Entity<RenewalReminder>()
            .HasOne(r => r.Subscription)
            .WithMany(s => s.RenewalReminders)
            .HasForeignKey(r => r.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // EngagementActivity -> Subscription
        // =====================================================

        modelBuilder.Entity<EngagementActivity>()
            .HasOne(a => a.Subscription)
            .WithMany()
            .HasForeignKey(a => a.SubscriptionId)
            .OnDelete(DeleteBehavior.NoAction);

        // =====================================================
        // EngagementActivity -> RenewalReminder
        // =====================================================

        modelBuilder.Entity<EngagementActivity>()
            .HasOne(a => a.RenewalReminder)
            .WithMany(r => r.Activities)
            .HasForeignKey(a => a.RenewalReminderId)
            .OnDelete(DeleteBehavior.NoAction);

        // =====================================================
        // RenewalReminder unique index
        // =====================================================

        modelBuilder.Entity<RenewalReminder>()
            .HasIndex(r => new
            {
                r.SubscriptionId,
                r.OffsetDays
            })
            .IsUnique();

        // =====================================================
        // Voucher
        // =====================================================

        modelBuilder.Entity<Voucher>()
            .HasIndex(v => v.Code)
            .IsUnique();

        // =====================================================
        // ProposalRevision -> Proposal
        // =====================================================

        modelBuilder.Entity<ProposalRevision>()
            .HasOne(revision => revision.Proposal)
            .WithMany()
            .HasForeignKey(revision => revision.ProposalId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ProposalRevision>()
            .HasIndex(revision => new
            {
                revision.ProposalId,
                revision.Revision
            })
            .IsUnique();

        // =====================================================
        // Employee indexes
        // =====================================================

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        // =====================================================
        // EmployeeGrade precision
        // =====================================================

        modelBuilder.Entity<EmployeeGrade>()
            .Property(g => g.ApprovalLimit)
            .HasPrecision(18, 2);

        // =====================================================
        // Product precision
        // =====================================================

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        // =====================================================
        // Opportunity indexes
        // =====================================================

        modelBuilder.Entity<Opportunity>()
            .HasIndex(o => new
            {
                o.CustomerId,
                o.Status
            });

        modelBuilder.Entity<Opportunity>()
            .HasIndex(o => o.AssignedEmployeeId);

        modelBuilder.Entity<Opportunity>()
            .HasIndex(o => o.AccountId);

        // =====================================================
        // Proposal indexes
        // =====================================================

        modelBuilder.Entity<Proposal>()
            .HasIndex(p => new
            {
                p.OpportunityId,
                p.Status
            });

        // =====================================================
        // ProposalProduct
        // =====================================================

        modelBuilder.Entity<ProposalProduct>()
            .HasIndex(p => new
            {
                p.ProposalId,
                p.ProductId
            })
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

        // =====================================================
        // OrderProduct
        // =====================================================

        modelBuilder.Entity<OrderProduct>()
            .HasIndex(p => new
            {
                p.OrderId,
                p.ProductId
            })
            .IsUnique();

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

        // =====================================================
        // OrderProduct -> Converted Opportunity
        // =====================================================

        modelBuilder.Entity<OrderProduct>()
            .HasOne(p => p.ConvertedOpportunity)
            .WithMany()
            .HasForeignKey(p => p.ConvertedOpportunityId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // OrderProduct -> Account
        // =====================================================

        modelBuilder.Entity<OrderProduct>()
            .HasOne(p => p.Account)
            .WithMany()
            .HasForeignKey(p => p.AccountId)
            .OnDelete(DeleteBehavior.SetNull);

        // =====================================================
        // Account
        // =====================================================

        modelBuilder.Entity<Account>()
            .HasKey(a => a.AccountId);

        modelBuilder.Entity<Account>()
            .Property(a => a.AccountId)
            .IsRequired();

        // =====================================================
        // Account -> AccountContacts
        // =====================================================

        modelBuilder.Entity<AccountContacts>()
            .HasOne(c => c.Account)
            .WithMany(a => a.Contacts)
            .HasForeignKey(c => c.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // Account -> AccountAddress
        // =====================================================

        modelBuilder.Entity<AccountAddress>()
            .HasOne(a => a.Account)
            .WithMany(account => account.Addresses)
            .HasForeignKey(a => a.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // =====================================================
        // AccountAddressType
        // =====================================================

        modelBuilder.Entity<AccountAddressType>()
            .HasKey(a => new
            {
                a.AccountAddressId,
                a.AddressType
            });

        modelBuilder.Entity<AccountAddressType>()
            .HasOne(a => a.AccountAddress)
            .WithMany(a => a.AddressTypes)
            .HasForeignKey(a => a.AccountAddressId)
            .OnDelete(DeleteBehavior.Cascade);

        // =====================================================
        // AccountContact indexes
        // =====================================================

        modelBuilder.Entity<AccountContacts>()
            .HasIndex(c => c.AccountId);

        // =====================================================
        // AccountAddress indexes
        // =====================================================

        modelBuilder.Entity<AccountAddress>()
            .HasIndex(a => a.AccountId);

        // =====================================================
        // Account indexes
        // =====================================================

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.AccountName);

        modelBuilder.Entity<Account>()
            .HasIndex(a => a.Email);

        // =====================================================
        // Proposal / Order unique indexes
        // =====================================================

        modelBuilder.Entity<Proposal>()
            .HasIndex(p => p.ProposalNumber)
            .IsUnique();

        modelBuilder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();
    }
}