using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Interfaces;

public interface IWorkflowRepository
{
    Task<IReadOnlyList<PlayBook.Domain.PlayBook>> GetPlayBooksAsync(
        CancellationToken cancellationToken = default);

    Task<PlayBook.Domain.PlayBook?> GetPlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<PlayBook.Domain.PlayBook?> GetPlayBookForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddPlayBookAsync(
        PlayBook.Domain.PlayBook playBook,
        CancellationToken cancellationToken = default);

    Task<WorkflowExecution?> GetWorkflowExecutionAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    void RemoveConditions(IEnumerable<Condition> conditions);

    void RemoveTransitions(IEnumerable<WorkflowTransition> transitions);

    void RemoveSteps(IEnumerable<PlayBookStep> steps);

    void AddSteps(IEnumerable<PlayBookStep> steps);

    void AddTransitions(IEnumerable<WorkflowTransition> transitions);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}