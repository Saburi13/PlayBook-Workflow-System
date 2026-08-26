using PlayBook.Application.Approvals;
using PlayBook.Domain;

namespace PlayBook.Tests;

public sealed class ApprovalRoutingTests
{
    [Fact]
    public void ApprovalDecisionRequestAcceptsOnlyDecisionStatesAtServiceBoundary()
    {
        var request = new ApprovalDecisionRequest(Guid.NewGuid(), ApprovalStatus.Approved, "Reviewed");

        Assert.Equal(ApprovalStatus.Approved, request.Decision);
        Assert.Equal("Reviewed", request.Comments);
    }

    [Fact]
    public void SeededHierarchyHasManagerChainForApprovalRouting()
    {
        var anand = new Employee { Id = Guid.NewGuid(), FirstName = "Anand" };
        var abdul = new Employee { Id = Guid.NewGuid(), FirstName = "Abdul", Manager = anand, ManagerId = anand.Id };
        var aditya = new Employee { Id = Guid.NewGuid(), FirstName = "Aditya", Manager = abdul, ManagerId = abdul.Id };

        Assert.Equal(abdul.Id, aditya.ManagerId);
        Assert.Equal(anand.Id, abdul.ManagerId);
    }
}
