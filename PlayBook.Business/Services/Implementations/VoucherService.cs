using PlayBook.Domain;
using PlayBook.Business.Services.Interfaces;

namespace PlayBook.Business.Services.Implementations;

public sealed record VoucherValidationResult(bool IsValid, string? Error, PricingVoucher? Voucher);

public sealed class VoucherService
{
    public VoucherValidationResult Validate(Voucher? voucher, decimal amount, DateTime now)
    {
        if (voucher is null) return new(false, "Voucher code is invalid.", null);
        if (!voucher.IsActive) return new(false, "Voucher is inactive.", null);
        if (voucher.ValidFrom.HasValue && now < voucher.ValidFrom.Value) return new(false, "Voucher is not yet valid.", null);
        if (voucher.ValidUntil.HasValue && now > voucher.ValidUntil.Value) return new(false, "Voucher has expired.", null);
        if (voucher.MinimumAmount.HasValue && amount < voucher.MinimumAmount.Value) return new(false, "The proposal amount does not meet the voucher minimum.", null);
        if (voucher.DiscountValue < 0m || voucher.DiscountType == DiscountType.Percentage && voucher.DiscountValue > 100m) return new(false, "Voucher discount is invalid.", null);
        return new(true, null, new PricingVoucher(voucher.DiscountType, voucher.DiscountValue, voucher.Stackable));
    }
}
