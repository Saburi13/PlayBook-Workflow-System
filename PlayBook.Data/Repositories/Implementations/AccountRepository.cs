using Microsoft.EntityFrameworkCore;
using PlayBook.Data.Context;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Data.Repositories.Implementations;

public sealed class AccountRepository(
    PlayBookDbContext dbContext) : IAccountRepository
{
    public IQueryable<Account> Query()
    {
        return dbContext.Accounts.AsQueryable();
    }

    public async Task<Account?> GetByIdAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Accounts
            .Include(a => a.Contacts)
            .Include(a => a.Addresses)
                .ThenInclude(a => a.AddressTypes)
            .FirstOrDefaultAsync(
                a => a.AccountId == accountId,
                cancellationToken);
    }

    public async Task AddAsync(
        Account account,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Accounts.AddAsync(account, cancellationToken);
    }

    public void Update(Account account)
    {
        dbContext.Accounts.Update(account);
    }

    public void Remove(Account account)
    {
        dbContext.Accounts.Remove(account);
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}