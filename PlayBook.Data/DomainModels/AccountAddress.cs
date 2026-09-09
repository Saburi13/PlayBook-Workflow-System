using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlayBook.Domain;

public class AccountAddress : AuditableEntity
{
    [Key]
    public Guid AccountAddressId { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public string? MapUrl { get; set; }

    public string? AddressLine { get; set; }

    public bool PrimaryAddress { get; set; }

    public bool IsDeleted { get; set; }

    public string AccountId { get; set; } = string.Empty;

    [ForeignKey(nameof(AccountId))]
    public Account Account { get; set; } = null!;

    public ICollection<AccountAddressType> AddressTypes { get; set; } = new List<AccountAddressType>();

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdatedDate { get; set; }
}
