namespace HospitalAppointment.Domain.ValueObjects;

// Para birimini ve miktarı temsil eden değer nesnesi — negatif değer kabul etmez
public sealed class Money : IEquatable<Money>
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money() { Amount = 0; Currency = "TRY"; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Para miktarı negatif olamaz.", nameof(amount));
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Para birimi boş olamaz.", nameof(currency));

        Amount = decimal.Round(amount, 2);
        Currency = currency.ToUpperInvariant();
    }

    public static Money Of(decimal amount, string currency = "TRY") => new(amount, currency);
    public static Money Zero(string currency = "TRY") => new(0, currency);

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount + other.Amount, Currency);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);
        return new Money(Amount - other.Amount, Currency);
    }

    public Money Multiply(decimal factor)
    {
        if (factor < 0)
            throw new ArgumentException("Çarpan negatif olamaz.", nameof(factor));
        return new Money(Amount * factor, Currency);
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException($"Farklı para birimleri toplanamaz: {Currency} ve {other.Currency}");
    }

    public bool Equals(Money? other)
    {
        if (other is null) return false;
        return Amount == other.Amount && Currency == other.Currency;
    }

    public override bool Equals(object? obj) => obj is Money money && Equals(money);
    public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    public override string ToString() => $"{Amount:F2} {Currency}";

    public static bool operator ==(Money left, Money right) => left.Equals(right);
    public static bool operator !=(Money left, Money right) => !left.Equals(right);
    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;
    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;
    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;
}
