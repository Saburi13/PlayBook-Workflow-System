using Microsoft.EntityFrameworkCore;
using PlayBook.Business.BusinessModels.RequestDTOs.AccountRequestDTOs;
using PlayBook.Business.BusinessModels.ResponseDTOs.AccountResponseDTOs;
using PlayBook.Business.Interfaces.IService;
using PlayBook.Data.Repositories.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Business.Implementations.Service;

public sealed class AccountService(
    IAccountRepository accountRepository) : IAccountService
{
    public async Task<IReadOnlyList<AccountDto>> GetAccountsAsync(
        CancellationToken cancellationToken = default)
    {
        var accounts = await accountRepository
            .Query()
            .Where(a => !a.IsDeleted)
            .Include(a => a.Contacts)
            .Include(a => a.Addresses)
                .ThenInclude(a => a.AddressTypes)
            .OrderBy(a => a.AccountName)
            .ToListAsync(cancellationToken);

        return accounts
            .Select(MapAccount)
            .ToList();
    }

    public async Task<AccountDto?> GetAccountAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetByIdAsync(
            accountId,
            cancellationToken);

        if (account is null || account.IsDeleted)
        {
            return null;
        }

        return MapAccount(account);
    }

    public async Task<AccountDto> CreateAccountAsync(
        AccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = new Account
        {
            AccountId = Guid.NewGuid().ToString(),

            AutoGenrateAccountId = request.AutoGenrateAccountId,
            AccountName = request.AccountName,
            RegisteredMobileNumber = request.RegisteredMobileNumber,
            SecondMobileNumber = request.SecondMobileNumber,
            Website = request.Website,
            Email = request.Email,
            AccountProfileImg = request.AccountProfileImg,
            Status = request.Status,
            IsDeleted = false,

            IncorporationDate = request.IncorporationDate,
            AccountSince = request.AccountSince,
            EmployeeCount = request.EmployeeCount,
            KeyAccount = request.KeyAccount,

            ReferralAccountId = request.ReferralAccountId,
            ParentAccountId = request.ParentAccountId,
            ReferralAccountContactsId = request.ReferralAccountContactsId,

            AccountTypesId = request.AccountTypesId,
            IndustryTypeId = request.IndustryTypeId,
            RegionId = request.RegionId,
            CurrencyId = request.CurrencyId,

            DefaultCurrencySymbol = request.DefaultCurrencySymbol,
            ConvertCurrencySymbol = request.ConvertCurrencySymbol,
            AccountManagerId = request.AccountManagerId,

            CreatedDate = DateTime.UtcNow,
            LastUpdatedDate = DateTime.UtcNow
        };

        await accountRepository.AddAsync(
            account,
            cancellationToken);

        await accountRepository.SaveChangesAsync(
            cancellationToken);

        return MapAccount(account);
    }

    public async Task<bool> UpdateAccountAsync(
        string accountId,
        AccountRequest request,
        CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetByIdAsync(
            accountId,
            cancellationToken);

        if (account is null || account.IsDeleted)
        {
            return false;
        }

        account.AutoGenrateAccountId = request.AutoGenrateAccountId;
        account.AccountName = request.AccountName;
        account.RegisteredMobileNumber = request.RegisteredMobileNumber;
        account.SecondMobileNumber = request.SecondMobileNumber;
        account.Website = request.Website;
        account.Email = request.Email;
        account.AccountProfileImg = request.AccountProfileImg;
        account.Status = request.Status;

        account.IncorporationDate = request.IncorporationDate;
        account.AccountSince = request.AccountSince;
        account.EmployeeCount = request.EmployeeCount;
        account.KeyAccount = request.KeyAccount;

        account.ReferralAccountId = request.ReferralAccountId;
        account.ParentAccountId = request.ParentAccountId;
        account.ReferralAccountContactsId = request.ReferralAccountContactsId;

        account.AccountTypesId = request.AccountTypesId;
        account.IndustryTypeId = request.IndustryTypeId;
        account.RegionId = request.RegionId;
        account.CurrencyId = request.CurrencyId;

        account.DefaultCurrencySymbol = request.DefaultCurrencySymbol;
        account.ConvertCurrencySymbol = request.ConvertCurrencySymbol;
        account.AccountManagerId = request.AccountManagerId;

        account.LastUpdatedDate = DateTime.UtcNow;

        accountRepository.Update(account);

        await accountRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAccountAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var account = await accountRepository.GetByIdAsync(
            accountId,
            cancellationToken);

        if (account is null || account.IsDeleted)
        {
            return false;
        }

        // Soft delete
        account.IsDeleted = true;
        account.LastUpdatedDate = DateTime.UtcNow;

        accountRepository.Update(account);

        await accountRepository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private static AccountDto MapAccount(Account account)
    {
        return new AccountDto(
            AccountId: account.AccountId,
            AutoGenrateAccountId: account.AutoGenrateAccountId,
            AccountName: account.AccountName,
            RegisteredMobileNumber: account.RegisteredMobileNumber,
            SecondMobileNumber: account.SecondMobileNumber,
            Website: account.Website,
            Email: account.Email,
            AccountProfileImg: account.AccountProfileImg,
            Status: account.Status,
            IsDeleted: account.IsDeleted,
            IncorporationDate: account.IncorporationDate,
            AccountSince: account.AccountSince,
            EmployeeCount: account.EmployeeCount,
            KeyAccount: account.KeyAccount,
            ReferralAccountId: account.ReferralAccountId,
            ParentAccountId: account.ParentAccountId,
            ReferralAccountContactsId: account.ReferralAccountContactsId,
            AccountTypesId: account.AccountTypesId,
            IndustryTypeId: account.IndustryTypeId,
            RegionId: account.RegionId,
            CurrencyId: account.CurrencyId,
            DefaultCurrencySymbol: account.DefaultCurrencySymbol,
            ConvertCurrencySymbol: account.ConvertCurrencySymbol,
            AccountManagerId: account.AccountManagerId,

            Contacts: account.Contacts
                .Where(c => !c.IsDeleted)
                .Select(c => new AccountContactDto(
                    AccountContactsId: c.AccountContactsId,
                    FirstName: c.FirstName,
                    LastName: c.LastName,
                    Email: c.Email,
                    MobileNumber: c.MobileNumber,
                    Designation: c.Designation,
                    Department: c.Department,
                    LinkedInProfile: c.LinkedInProfile,
                    IsPrimary: c.IsPrimary,
                    IsDeleted: c.IsDeleted))
                .ToList(),

            Addresses: account.Addresses
                .Where(a => !a.IsDeleted)
                .Select(a => new AccountAddressDto(
                    AccountAddressId: a.AccountAddressId,
                    Country: a.Country,
                    City: a.City,
                    State: a.State,
                    Pincode: a.Pincode,
                    MapUrl: a.MapUrl,
                    AddressLine: a.AddressLine,
                    PrimaryAddress: a.PrimaryAddress,
                    IsDeleted: a.IsDeleted))
                .ToList());
    }
}