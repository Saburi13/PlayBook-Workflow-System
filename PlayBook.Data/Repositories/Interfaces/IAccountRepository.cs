using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Interfaces;

public interface IAccountRepository
{
    IQueryable<Account> Query();

    Task<Account?> GetByIdAsync(
        string accountId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default);

    void Update(Account account);

    void Remove(Account account);

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}