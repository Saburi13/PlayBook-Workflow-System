using PlayBook.Business.BusinessModels.RequestDTOs.AccountRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.AccountResponseDTOs;

namespace PlayBook.Business.Interfaces.IService;

public interface IAccountService
{
    Task<IReadOnlyList<AccountDto>> GetAccountsAsync(
        CancellationToken cancellationToken = default);

    Task<AccountDto?> GetAccountAsync(
        string accountId,
        CancellationToken cancellationToken = default);

    Task<AccountDto> CreateAccountAsync(
        AccountRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAccountAsync(
        string accountId,
        AccountRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAccountAsync(
        string accountId,
        CancellationToken cancellationToken = default);
}