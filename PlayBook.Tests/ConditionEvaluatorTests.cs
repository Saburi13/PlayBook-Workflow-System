using System.Text.Json;
using PlayBook.Application.Services;

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
}