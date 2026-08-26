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
    }

    private static Product Product(string name, string description, string category, decimal price) =>
        new() { Id = Guid.NewGuid(), Name = name, Description = description, Category = category, Price = price };
}
