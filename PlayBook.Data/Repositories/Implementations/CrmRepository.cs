using Microsoft.EntityFrameworkCore;
using PlayBook.Data.Context;
using PlayBook.Data.Repositories.Interfaces;

namespace PlayBook.Data.Repositories.Implementations;

public sealed class CrmRepository<TEntity>(PlayBookDbContext dbContext) : ICrmRepository<TEntity>
    where TEntity : class
{
    public IQueryable<TEntity> Query() => dbContext.Set<TEntity>().AsQueryable();

    public Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        dbContext.Set<TEntity>().FindAsync([id], cancellationToken).AsTask();

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        dbContext.Set<TEntity>().AddAsync(entity, cancellationToken).AsTask();

    public void Update(TEntity entity) => dbContext.Set<TEntity>().Update(entity);

    public void Remove(TEntity entity) => dbContext.Set<TEntity>().Remove(entity);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
