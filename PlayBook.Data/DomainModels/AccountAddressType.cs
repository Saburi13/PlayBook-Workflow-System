using System.ComponentModel.DataAnnotations.Schema;

namespace PlayBook.Domain;

public class AccountAddressType
{
    public Guid AccountAddressId { get; set; }

    [ForeignKey(nameof(AccountAddressId))]
    public AccountAddress AccountAddress { get; set; } = null!;

    public AddressType AddressType { get; set; }
}
