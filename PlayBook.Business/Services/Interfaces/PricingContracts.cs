using PlayBook.Domain;

namespace PlayBook.Business.Services.Interfaces;

public sealed record PricingLine(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    DiscountType DiscountType = DiscountType.Percentage,
    decimal DiscountValue = 0m);

public sealed record PricingVoucher(
    DiscountType DiscountType,
    decimal DiscountValue,
    bool Stackable);

public sealed record CalculatedLine(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    decimal DiscountAmount,
    decimal TotalAmount,
    DiscountType DiscountType,
    decimal DiscountValue);

public sealed record PricingResult(
    decimal Subtotal,
    decimal LineDiscountAmount,
    decimal VoucherDiscountAmount,
    decimal TotalAmount,
    IReadOnlyList<CalculatedLine>? Lines = null);

public interface IPricingService
{
    PricingResult Calculate(
        IEnumerable<PricingLine> lines,
        IEnumerable<PricingVoucher>? vouchers = null);
}