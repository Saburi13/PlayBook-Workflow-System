using PlayBook.Application.Pricing;
using PlayBook.Domain;

namespace PlayBook.Tests;

public sealed class PricingTests
{
    private readonly PricingCalculator calculator = new();

    [Fact]
    public void CalculatesLineAndPercentageDiscount()
    {
        var result = calculator.Calculate([new PricingLine(Guid.NewGuid(), 2, 10m, DiscountType.Percentage, 10m)]);

        Assert.Equal(20m, result.Subtotal);
        Assert.Equal(2m, result.LineDiscountAmount);
        Assert.Equal(18m, result.TotalAmount);
    }

    [Fact]
    public void AppliesFixedDiscountAndRoundsAwayFromZero()
    {
        var result = calculator.Calculate([new PricingLine(Guid.NewGuid(), 3, 3.333m, DiscountType.FixedAmount, 1.005m)]);

        Assert.Equal(10m, result.Subtotal);
        Assert.Equal(1.01m, result.LineDiscountAmount);
        Assert.Equal(8.99m, result.TotalAmount);
    }

    [Fact]
    public void PreventsDiscountsFromProducingNegativeTotals()
    {
        var result = calculator.Calculate(
            [new PricingLine(Guid.NewGuid(), 1, 5m, DiscountType.FixedAmount, 99m)],
            [new PricingVoucher(DiscountType.FixedAmount, 99m, true)]);

        Assert.Equal(0m, result.TotalAmount);
        Assert.Equal(5m, result.LineDiscountAmount);
        Assert.Equal(0m, result.VoucherDiscountAmount);
    }

    [Fact]
    public void AppliesNonStackableOnlyOnceAndStackableVouchersSequentially()
    {
        var nonStacked = calculator.Calculate([new PricingLine(Guid.NewGuid(), 1, 100m)], [
            new PricingVoucher(DiscountType.Percentage, 10m, false),
            new PricingVoucher(DiscountType.FixedAmount, 5m, false)]);
        var stacked = calculator.Calculate([new PricingLine(Guid.NewGuid(), 1, 100m)], [
            new PricingVoucher(DiscountType.Percentage, 10m, true),
            new PricingVoucher(DiscountType.FixedAmount, 5m, true)]);

        Assert.Equal(90m, nonStacked.TotalAmount);
        Assert.Equal(85m, stacked.TotalAmount);
    }

    [Fact]
    public void VoucherServiceValidatesCodeStateDatesAndMinimumAmount()
    {
        var now = new DateTime(2026, 8, 28, 0, 0, 0, DateTimeKind.Utc);
        var service = new VoucherService();
        var voucher = new Voucher { Code = "TEST", DiscountType = DiscountType.Percentage, DiscountValue = 10m, MinimumAmount = 100m, ValidFrom = now.AddDays(-1), ValidUntil = now.AddDays(1) };

        Assert.True(service.Validate(voucher, 100m, now).IsValid);
        Assert.False(service.Validate(null, 100m, now).IsValid);
        Assert.False(service.Validate(new Voucher { IsActive = false }, 100m, now).IsValid);
        Assert.False(service.Validate(new Voucher { IsActive = true, ValidFrom = now.AddDays(1) }, 100m, now).IsValid);
        Assert.False(service.Validate(new Voucher { IsActive = true, ValidUntil = now.AddDays(-1) }, 100m, now).IsValid);
        Assert.False(service.Validate(voucher, 99m, now).IsValid);
    }
}
