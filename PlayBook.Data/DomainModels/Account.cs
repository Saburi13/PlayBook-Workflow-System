using System.ComponentModel.DataAnnotations;

namespace PlayBook.Domain;

public class Account : AuditableEntity
{
    [Key]
    public string AccountId { get; set; } = Guid.NewGuid().ToString();

    public string? AutoGenrateAccountId { get; set; }

    public string? AccountName { get; set; }

    public string? RegisteredMobileNumber { get; set; }

    public string? SecondMobileNumber { get; set; }

    public string? Website { get; set; }

    public string? Email { get; set; }

    public string? AccountProfileImg { get; set; }

    public string? Status { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? IncorporationDate { get; set; }

    public DateTime? AccountSince { get; set; }

    public int? EmployeeCount { get; set; }

    public bool KeyAccount { get; set; }

    public string? ReferralAccountId { get; set; }

    public string? ParentAccountId { get; set; }

    public Guid? ReferralAccountContactsId { get; set; }

    public Guid? AccountTypesId { get; set; }

    public Guid? IndustryTypeId { get; set; }

    public Guid? RegionId { get; set; }

    public Guid? CurrencyId { get; set; }

    public string? DefaultCurrencySymbol { get; set; }

    public string? ConvertCurrencySymbol { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }

    public string? CreatedByUserId { get; set; }

    public string? AccountManagerId { get; set; }

    public ICollection<AccountContacts> Contacts { get; set; } = new List<AccountContacts>();

    public ICollection<AccountAddress> Addresses { get; set; } = new List<AccountAddress>();
}
