using HospitalAppointment.Domain.ValueObjects;
using FluentAssertions;

namespace HospitalAppointment.Domain.Tests.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Of_PositiveAmount_CreatesCorrectly()
    {
        var money = Money.Of(100m, "TRY");

        money.Amount.Should().Be(100m);
        money.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Of_NegativeAmount_ThrowsArgumentException()
    {
        var act = () => Money.Of(-10m, "TRY");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*negatif*");
    }

    [Fact]
    public void Add_SameCurrency_ReturnsSum()
    {
        var a = Money.Of(100m);
        var b = Money.Of(50m);

        var result = a.Add(b);

        result.Amount.Should().Be(150m);
        result.Currency.Should().Be("TRY");
    }

    [Fact]
    public void Add_DifferentCurrency_ThrowsInvalidOperationException()
    {
        var tryMoney = Money.Of(100m, "TRY");
        var usdMoney = Money.Of(50m, "USD");

        var act = () => tryMoney.Add(usdMoney);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiply_PositiveFactor_ReturnsCorrectAmount()
    {
        var money = Money.Of(100m);

        var result = money.Multiply(1.18m);

        result.Amount.Should().Be(118m);
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        var a = Money.Of(100m, "TRY");
        var b = Money.Of(100m, "TRY");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void Zero_ReturnsZeroAmount()
    {
        var zero = Money.Zero();

        zero.Amount.Should().Be(0m);
    }

    [Theory]
    [InlineData(100, 50, true)]
    [InlineData(50, 100, false)]
    [InlineData(100, 100, false)]
    public void GreaterThan_ReturnsCorrectResult(decimal left, decimal right, bool expected)
    {
        var a = Money.Of(left);
        var b = Money.Of(right);

        (a > b).Should().Be(expected);
    }
}
