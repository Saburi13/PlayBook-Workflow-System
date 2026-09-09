using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayBook.Domain;

public class AccountContacts : AuditableEntity
{
    [Key]
    public Guid AccountContactsId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? MobileNumber { get; set; }

    public string? Designation { get; set; }

    public string? Department { get; set; }

    public string? LinkedInProfile { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsDeleted { get; set; }

    public string AccountId { get; set; } = string.Empty;

    [ForeignKey(nameof(AccountId))]
    public Account Account { get; set; } = null!;

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }
}
