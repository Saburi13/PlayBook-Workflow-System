using Microsoft.EntityFrameworkCore;
using PlayBook.Domain;

namespace PlayBook.Infrastructure.Data;

public static class DevelopmentDataSeeder
{
    private static readonly Guid ExecutiveGradeId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid ManagerGradeId = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid SalesGradeId = Guid.Parse("10000000-0000-0000-0000-000000000003");
    private static readonly Guid AnandId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid AbdulId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid AdityaId = Guid.Parse("20000000-0000-0000-0000-000000000003");
    private static readonly Guid CustomerId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid OpportunityId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private static readonly Guid DemoPlayBookId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    private static readonly Guid DemoEventPlayBookId = Guid.Parse("50000000-0000-0000-0000-000000000002");
    private static readonly Guid DemoProposalId = Guid.Parse("60000000-0000-0000-0000-000000000001");
    private static readonly Guid DemoPercentageVoucherId = Guid.Parse("70000000-0000-0000-0000-000000000001");
    private static readonly Guid DemoFixedVoucherId = Guid.Parse("70000000-0000-0000-0000-000000000002");
    private static readonly Guid DemoExpiredVoucherId = Guid.Parse("70000000-0000-0000-0000-000000000003");

    public static async Task SeedAsync(PlayBookDbContext db, CancellationToken cancellationToken = default)
    {
        if (!await db.EmployeeGrades.AnyAsync(cancellationToken))
        {
            db.EmployeeGrades.AddRange(
                new EmployeeGrade { Id = ExecutiveGradeId, Name = "Executive", Description = "Executive approval tier", ApprovalLimit = 1000000 },
                new EmployeeGrade { Id = ManagerGradeId, Name = "Manager", Description = "Manager approval tier", ApprovalLimit = 250000 },
                new EmployeeGrade { Id = SalesGradeId, Name = "Sales Representative", Description = "Sales team tier", ApprovalLimit = 50000 });
        }

        if (!await db.Employees.AnyAsync(cancellationToken))
        {
            db.Employees.AddRange(
                new Employee { Id = AnandId, FirstName = "Anand", LastName = "Krishnan", Email = "anand@playbook.local", EmployeeGradeId = ExecutiveGradeId, Role = EmployeeRole.Admin },
                new Employee { Id = AbdulId, FirstName = "Abdul", LastName = "Rahman", Email = "abdul@playbook.local", EmployeeGradeId = ManagerGradeId, ManagerId = AnandId, Role = EmployeeRole.Manager },
                new Employee { Id = AdityaId, FirstName = "Aditya", LastName = "Sharma", Email = "aditya@playbook.local", EmployeeGradeId = SalesGradeId, ManagerId = AbdulId, Role = EmployeeRole.Employee });
        }

        if (!await db.Products.AnyAsync(cancellationToken))
        {
            db.Products.AddRange(
                Product("Internet 100 Mbps", "Business broadband plan", "Connectivity", 1499),
                Product("Netflix", "Streaming subscription", "Entertainment", 649),
                Product("Hotstar", "Streaming subscription", "Entertainment", 299),
                Product("Prime", "Streaming subscription", "Entertainment", 299),
                Product("Router", "Managed Wi-Fi router", "Hardware", 2499),
                Product("Installation", "Professional installation service", "Services", 999));
        }

        if (!await db.Customers.AnyAsync(cancellationToken))
        {
            db.Customers.Add(new Customer { Id = CustomerId, Name = "Northwind Retail", Company = "Northwind Retail Pvt Ltd", Email = "contact@northwind.local", Phone = "+91 90000 10001", Address = "Bengaluru, Karnataka", Status = "Active" });
        }

        if (!await db.Opportunities.AnyAsync(cancellationToken))
        {
            db.Opportunities.Add(new Opportunity { Id = OpportunityId, CustomerId = CustomerId, AssignedEmployeeId = AdityaId, Name = "Northwind connectivity upgrade", Description = "100 Mbps connectivity with managed router and entertainment bundle", EstimatedValue = 54999, Status = OpportunityStatus.InProgress, ExpectedCloseDate = DateTime.UtcNow.Date.AddDays(30) });
        }

        await db.SaveChangesAsync(cancellationToken);

        if (!await db.Vouchers.AnyAsync(item => item.Code == "DEMO-PERCENT", cancellationToken))
        {
            db.Vouchers.Add(new Voucher { Id = DemoPercentageVoucherId, Code = "DEMO-PERCENT", DiscountType = DiscountType.Percentage, DiscountValue = 10m, IsActive = true, MinimumAmount = 1000m, Stackable = false });
        }
        if (!await db.Vouchers.AnyAsync(item => item.Code == "DEMO-FIXED", cancellationToken))
        {
            db.Vouchers.Add(new Voucher { Id = DemoFixedVoucherId, Code = "DEMO-FIXED", DiscountType = DiscountType.FixedAmount, DiscountValue = 250m, IsActive = true, MinimumAmount = 2500m, Stackable = true });
        }
        if (!await db.Vouchers.AnyAsync(item => item.Code == "DEMO-EXPIRED", cancellationToken))
        {
            db.Vouchers.Add(new Voucher { Id = DemoExpiredVoucherId, Code = "DEMO-EXPIRED", DiscountType = DiscountType.Percentage, DiscountValue = 15m, IsActive = true, ValidUntil = DateTime.UtcNow.AddDays(-1) });
        }
        await db.SaveChangesAsync(cancellationToken);

        if (!await db.PlayBooks.AnyAsync(cancellationToken))
        {
            var internetProduct = await db.Products.SingleAsync(item => item.Name == "Internet 100 Mbps", cancellationToken);
            var routerProduct = await db.Products.SingleAsync(item => item.Name == "Router", cancellationToken);

            var startStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Start", Description = "Begin proposal review", StepType = StepType.Trigger, PositionX = 80, PositionY = 120, IsStartStep = true, IsEndStep = false };
            var approvalStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Manager approval", Description = "Manager approval required", StepType = StepType.Approval, PositionX = 300, PositionY = 120, IsStartStep = false, IsEndStep = false };
            var customerStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Customer approval", Description = "Customer confirms the proposal", StepType = StepType.CustomerAction, PositionX = 540, PositionY = 120, IsStartStep = false, IsEndStep = false };
            var endStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Completed", Description = "Flow complete", StepType = StepType.End, PositionX = 760, PositionY = 120, IsStartStep = false, IsEndStep = true };

            var playBook = new PlayBook.Domain.PlayBook
            {
                Id = DemoPlayBookId,
                Name = "Proposal approval and subscription flow",
                Description = "Starts with a proposal submission, obtains internal approval, and then waits for customer approval before creating subscriptions.",
                Version = 1,
                Status = PlayBookStatus.Active,
                TriggerType = TriggerType.Manual,
                CreatedBy = "system",
                Steps = [startStep, approvalStep, customerStep, endStep],
                Transitions =
                [
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = startStep.Id, ToStepId = approvalStep.Id, Priority = 1, Label = "Submit" },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = approvalStep.Id, ToStepId = customerStep.Id, Priority = 1, Label = "Approved" },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = customerStep.Id, ToStepId = endStep.Id, Priority = 1, Label = "Confirmed" }
                ]
            };

            startStep.PlayBook = playBook;
            approvalStep.PlayBook = playBook;
            customerStep.PlayBook = playBook;
            endStep.PlayBook = playBook;

            foreach (var transition in playBook.Transitions)
            {
                transition.PlayBook = playBook;
            }

            db.PlayBooks.Add(playBook);
        }

        await db.SaveChangesAsync(cancellationToken);

        var demoEventPlayBook = await db.PlayBooks
            .Include(item => item.Steps)
            .Include(item => item.Transitions)
            .SingleOrDefaultAsync(item => item.Id == DemoEventPlayBookId, cancellationToken);

        if (demoEventPlayBook is null)
        {
            var triggerStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Opportunity Created", Description = "Start when an opportunity is created", StepType = StepType.Trigger, ConfigurationJson = "{\"triggerType\":\"Event\",\"event\":\"Opportunity Created\"}", PositionX = 80, PositionY = 180, IsStartStep = true };
            var proposalStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Create Proposal", Description = "Create a proposal from the opportunity", StepType = StepType.Action, ConfigurationJson = "{\"actionType\":\"Create Proposal\"}", PositionX = 300, PositionY = 180 };
            var conditionStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Check Discount", Description = "Route discounts greater than five percent", StepType = StepType.Condition, ConfigurationJson = "{\"field\":\"Proposal.DiscountPercentage\",\"operator\":\"GreaterThan\",\"value\":\"5\",\"dataType\":\"decimal\"}", PositionX = 520, PositionY = 180 };
            var managerStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Manager Approval", Description = "Manager approval for high discounts", StepType = StepType.Approval, ConfigurationJson = "{\"approverType\":\"Manager\",\"approvalLevel\":1}", PositionX = 740, PositionY = 60 };
            var autoApprovalStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Auto Approval", Description = "Automatically approve standard discounts", StepType = StepType.Action, ConfigurationJson = "{\"actionType\":\"Auto Approval\"}", PositionX = 740, PositionY = 300 };
            var customerStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Customer Approval", Description = "Customer confirms the proposal", StepType = StepType.CustomerAction, ConfigurationJson = "{\"action\":\"Customer Approval\"}", PositionX = 960, PositionY = 180 };
            var orderStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Create Order", Description = "Create the confirmed order", StepType = StepType.Action, ConfigurationJson = "{\"actionType\":\"Create Order\"}", PositionX = 1180, PositionY = 180 };
            var endStep = new PlayBookStep { Id = Guid.NewGuid(), Name = "Completed", Description = "Flow complete", StepType = StepType.End, PositionX = 1400, PositionY = 180, IsEndStep = true };
            var playBook = new PlayBook.Domain.PlayBook
            {
                Id = DemoEventPlayBookId, Name = "Opportunity proposal approval demo", Description = "Event-driven opportunity to proposal, approval, order, and subscription flow.", Version = 1,
                Status = PlayBookStatus.Active, TriggerType = TriggerType.Event, CreatedBy = "system",
                Steps = [triggerStep, proposalStep, conditionStep, managerStep, autoApprovalStep, customerStep, orderStep, endStep],
                Transitions =
                [
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = triggerStep.Id, ToStepId = proposalStep.Id, Label = "Next", Priority = 0 },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = proposalStep.Id, ToStepId = conditionStep.Id, Label = "Next", Priority = 1 },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = conditionStep.Id, ToStepId = managerStep.Id, Label = "TRUE", Priority = 2, Condition = new Condition { Id = Guid.NewGuid(), Field = "Proposal.DiscountPercentage", Operator = ConditionOperator.GreaterThan, Value = "5", DataType = "decimal", StepId = conditionStep.Id } },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = conditionStep.Id, ToStepId = autoApprovalStep.Id, Label = "FALSE", Priority = 3, Condition = new Condition { Id = Guid.NewGuid(), Field = "Proposal.DiscountPercentage", Operator = ConditionOperator.GreaterThan, Value = "5", DataType = "decimal", StepId = conditionStep.Id } },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = managerStep.Id, ToStepId = customerStep.Id, Label = "Approved", Priority = 4 },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = autoApprovalStep.Id, ToStepId = customerStep.Id, Label = "Approved", Priority = 5 },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = customerStep.Id, ToStepId = orderStep.Id, Label = "Confirmed", Priority = 6 },
                    new WorkflowTransition { Id = Guid.NewGuid(), FromStepId = orderStep.Id, ToStepId = endStep.Id, Label = "Complete", Priority = 7 }
                ]
            };
            db.PlayBooks.Add(playBook);
        }
        else
        {
            demoEventPlayBook.Status = PlayBookStatus.Active;
            demoEventPlayBook.TriggerType = TriggerType.Event;
        }

        if (!await db.Proposals.AnyAsync(cancellationToken))
        {
            var opportunity = await db.Opportunities.SingleAsync(item => item.Id == OpportunityId, cancellationToken);
            var customer = await db.Customers.SingleAsync(item => item.Id == CustomerId, cancellationToken);
            var employee = await db.Employees.SingleAsync(item => item.Id == AdityaId, cancellationToken);
            var internetProduct = await db.Products.SingleAsync(item => item.Name == "Internet 100 Mbps", cancellationToken);
            var routerProduct = await db.Products.SingleAsync(item => item.Name == "Router", cancellationToken);

            var proposal = new Proposal
            {
                Id = DemoProposalId,
                OpportunityId = opportunity.Id,
                CustomerId = customer.Id,
                CreatedByEmployeeId = employee.Id,
                ProposalNumber = "P-1001",
                Status = ProposalStatus.Draft,
                SubTotal = 1749m,
                DiscountPercentage = 0m,
                DiscountAmount = 0m,
                TotalAmount = 1749m,
                ValidUntil = DateTime.UtcNow.AddDays(30),
                Opportunity = opportunity,
                Customer = customer,
                CreatedByEmployee = employee
            };

            db.Proposals.Add(proposal);
            await db.SaveChangesAsync(cancellationToken);

            db.ProposalProducts.AddRange(
                new ProposalProduct { Id = Guid.NewGuid(), ProposalId = proposal.Id, ProductId = internetProduct.Id, Quantity = 1, UnitPrice = internetProduct.Price, DiscountPercentage = 0m, DiscountAmount = 0m, TotalPrice = internetProduct.Price },
                new ProposalProduct { Id = Guid.NewGuid(), ProposalId = proposal.Id, ProductId = routerProduct.Id, Quantity = 1, UnitPrice = routerProduct.Price, DiscountPercentage = 0m, DiscountAmount = 0m, TotalPrice = routerProduct.Price });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Product Product(string name, string description, string category, decimal price) =>
        new() { Id = Guid.NewGuid(), Name = name, Description = description, Category = category, Price = price };
}
