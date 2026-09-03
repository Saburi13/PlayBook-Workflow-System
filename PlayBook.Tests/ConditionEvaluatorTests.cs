using System.Text.Json;
using PlayBook.Business.Services.Implementations;
using PlayBook.Domain;

namespace PlayBook.Tests;

public sealed class ConditionEvaluatorTests
{
    private readonly ConditionEvaluator evaluator = new();

    [Fact]
    public void EvaluatesNestedJsonProperty()
    {
        using var document = JsonDocument.Parse("{\"customer\":{\"segment\":\"Enterprise\"}}");

        var result = evaluator.Evaluate("customer.segment", "Equals", "enterprise", document.RootElement);

        Assert.True(result);
    }

    [Fact]
    public void EvaluatesJsonNumberComparison()
    {
        using var document = JsonDocument.Parse("{\"amount\":125000}");

        var result = evaluator.Evaluate("amount", "GreaterThan", "100000", document.RootElement, "decimal");

        Assert.True(result);
    }

    [Fact]
    public void MissingJsonPropertyDoesNotMatch()
    {
        using var document = JsonDocument.Parse("{\"status\":\"Draft\"}");

        var result = evaluator.Evaluate("priority", "Equals", "High", document.RootElement);

        Assert.False(result);
    }

    [Fact]
    public void EvaluatesEntityPrefixedProperty()
    {
        var proposal = new Proposal { DiscountPercentage = 10m };

        var result = evaluator.Evaluate("Proposal.DiscountPercentage", "GreaterThan", "5", proposal, "decimal");

        Assert.True(result);
    }
}