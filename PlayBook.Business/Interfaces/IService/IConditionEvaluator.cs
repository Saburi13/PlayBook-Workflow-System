namespace PlayBook.Business.Interfaces.IService;

public interface IConditionEvaluator
{
    bool Evaluate(string field, string @operator, string? value, object? model, string? dataType = "string");
}
