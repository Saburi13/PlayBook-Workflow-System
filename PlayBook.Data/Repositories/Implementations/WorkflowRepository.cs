using Microsoft.EntityFrameworkCore;
using PlayBook.Data.Context;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Implementations;

public sealed class WorkflowRepository(
    PlayBookDbContext dbContext) : IWorkflowRepository
{
    public async Task<IReadOnlyList<PlayBook.Domain.PlayBook>> GetPlayBooksAsync(
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PlayBooks
            .AsNoTracking()
            .OrderBy(playBook => playBook.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<PlayBook.Domain.PlayBook?> GetPlayBookAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PlayBooks
            .AsNoTracking()
            .Include(playBook => playBook.Steps)
            .Include(playBook => playBook.Transitions)
                .ThenInclude(transition => transition.Condition)
            .AsSplitQuery()
            .SingleOrDefaultAsync(
                playBook => playBook.Id == id,
                cancellationToken);
    }

    public async Task<PlayBook.Domain.PlayBook?> GetPlayBookForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.PlayBooks
            .Include(playBook => playBook.Steps)
                .ThenInclude(step => step.Conditions)
            .Include(playBook => playBook.Transitions)
                .ThenInclude(transition => transition.Condition)
            .SingleOrDefaultAsync(
                playBook => playBook.Id == id,
                cancellationToken);
    }

    public Task AddPlayBookAsync(
        PlayBook.Domain.PlayBook playBook,
        CancellationToken cancellationToken = default)
    {
        return dbContext.PlayBooks
            .AddAsync(playBook, cancellationToken)
            .AsTask();
    }

    public async Task<WorkflowExecution?> GetWorkflowExecutionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.WorkflowExecutions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                execution => execution.Id == id,
                cancellationToken);
    }

    public void RemoveConditions(
        IEnumerable<Condition> conditions)
    {
        dbContext.Conditions.RemoveRange(conditions);
    }

    public void RemoveTransitions(
        IEnumerable<WorkflowTransition> transitions)
    {
        dbContext.WorkflowTransitions.RemoveRange(transitions);
    }

    public void RemoveSteps(
        IEnumerable<PlayBookStep> steps)
    {
        dbContext.PlayBookSteps.RemoveRange(steps);
    }

    public void AddSteps(
        IEnumerable<PlayBookStep> steps)
    {
        dbContext.PlayBookSteps.AddRange(steps);
    }

    public void AddTransitions(
        IEnumerable<WorkflowTransition> transitions)
    {
        dbContext.WorkflowTransitions.AddRange(transitions);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}