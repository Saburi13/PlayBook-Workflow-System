namespace PlayBook.Business.BusinessModels.ResponseDTOs.AccountResponseDTOs;

public sealed record AccountDto(
    string AccountId,
    string? AutoGenrateAccountId,
    string? AccountName,
    string? RegisteredMobileNumber,
    string? SecondMobileNumber,
    string? Website,
    string? Email,
    string? AccountProfileImg,
    string? Status,
    bool IsDeleted,
    DateTime? IncorporationDate,
    DateTime? AccountSince,
    int? EmployeeCount,
    bool KeyAccount,
    string? ReferralAccountId,
    string? ParentAccountId,
    Guid? ReferralAccountContactsId,
    Guid? AccountTypesId,
    Guid? IndustryTypeId,
    Guid? RegionId,
    Guid? CurrencyId,
    string? DefaultCurrencySymbol,
    string? ConvertCurrencySymbol,
    string? AccountManagerId,
    IReadOnlyList<AccountContactDto> Contacts,
    IReadOnlyList<AccountAddressDto> Addresses);

public sealed record AccountContactDto(
    Guid AccountContactsId,
    string? FirstName,
    string? LastName,
    string? Email,
    string? MobileNumber,
    string? Designation,
    string? Department,
    string? LinkedInProfile,
    bool IsPrimary,
    bool IsDeleted);

public sealed record AccountAddressDto(
    Guid AccountAddressId,
    string? Country,
    string? City,
    string? State,
    string? Pincode,
    string? MapUrl,
    string? AddressLine,
    bool PrimaryAddress,
    bool IsDeleted);

public sealed record AccountAddressTypeDto(
    Guid AccountAddressId,
    int AddressType);