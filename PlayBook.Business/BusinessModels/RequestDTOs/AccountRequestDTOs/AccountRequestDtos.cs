namespace PlayBook.Business.BusinessModels.RequestDTOs.AccountRequestDTOs;

public sealed record AccountRequest(
    string? AutoGenrateAccountId,
    string AccountName,
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
    string? AccountManagerId);

public sealed record AccountContactRequest(
    string? FirstName,
    string? LastName,
    string? Email,
    string? MobileNumber,
    string? Designation,
    string? Department,
    string? LinkedInProfile,
    bool IsPrimary,
    bool IsDeleted);

public sealed record AccountAddressRequest(
    string? Country,
    string? City,
    string? State,
    string? Pincode,
    string? MapUrl,
    string? AddressLine,
    bool PrimaryAddress,
    bool IsDeleted);

public sealed record AccountAddressTypeRequest(
    Guid AccountAddressId,
    int AddressType);