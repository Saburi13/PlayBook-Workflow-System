using PlayBook.Business.Services.Interfaces;
using PlayBook.Domain;

namespace PlayBook.Business.Services.Implementations;

public sealed class PricingCalculator : IPricingService
{
    public PricingResult Calculate(IEnumerable<PricingLine> lines, IEnumerable<PricingVoucher>? vouchers = null)
    {
        var calculatedLines = lines.Select(line =>
        {
            var subtotal = decimal.Round(Math.Max(0m, line.Quantity) * Math.Max(0m, line.UnitPrice), 2, MidpointRounding.AwayFromZero);
            var discount = line.DiscountType == DiscountType.Percentage
                ? subtotal * Math.Clamp(line.DiscountValue, 0m, 100m) / 100m
                : Math.Clamp(line.DiscountValue, 0m, subtotal);
            return new CalculatedLine(line.ProductId, line.Quantity, line.UnitPrice, subtotal, decimal.Round(discount, 2, MidpointRounding.AwayFromZero), decimal.Round(Math.Max(0m, subtotal - discount), 2, MidpointRounding.AwayFromZero), line.DiscountType, line.DiscountValue);
        }).ToList();
        var subtotalTotal = decimal.Round(calculatedLines.Sum(line => line.Subtotal), 2, MidpointRounding.AwayFromZero);
        var lineDiscount = decimal.Round(calculatedLines.Sum(line => line.DiscountAmount), 2, MidpointRounding.AwayFromZero);
        var runningTotal = Math.Max(0m, subtotalTotal - lineDiscount);
        var voucherDiscount = 0m;
        foreach (var voucher in (vouchers ?? []).Where(voucher => voucher.Stackable || voucherDiscount == 0m))
        {
            var discount = voucher.DiscountType == DiscountType.Percentage
                ? runningTotal * Math.Clamp(voucher.DiscountValue, 0m, 100m) / 100m
                : Math.Clamp(voucher.DiscountValue, 0m, runningTotal);
            discount = decimal.Round(Math.Min(runningTotal, Math.Max(0m, discount)), 2, MidpointRounding.AwayFromZero);
            runningTotal = Math.Max(0m, runningTotal - discount);
            voucherDiscount += discount;
        }
        return new PricingResult(subtotalTotal, lineDiscount, decimal.Round(voucherDiscount, 2, MidpointRounding.AwayFromZero), decimal.Round(runningTotal, 2, MidpointRounding.AwayFromZero), calculatedLines);
    }
}
