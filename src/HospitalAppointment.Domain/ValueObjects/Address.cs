namespace HospitalAppointment.Domain.ValueObjects;

// Adres bilgisini kapsülleyen değer nesnesi
public sealed class Address : IEquatable<Address>
{
    public string Street { get; }
    public string District { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    private Address() { Street = ""; District = ""; City = ""; PostalCode = ""; Country = ""; }

    private Address(string street, string district, string city, string postalCode, string country)
    {
        Street = street?.Trim() ?? throw new ArgumentNullException(nameof(street));
        District = district?.Trim() ?? throw new ArgumentNullException(nameof(district));
        City = city?.Trim() ?? throw new ArgumentNullException(nameof(city));
        PostalCode = postalCode?.Trim() ?? throw new ArgumentNullException(nameof(postalCode));
        Country = country?.Trim() ?? throw new ArgumentNullException(nameof(country));
    }

    public static Address Of(string street, string district, string city, string postalCode, string country = "Türkiye")
        => new(street, district, city, postalCode, country);

    public string GetFullAddress() => $"{Street}, {District}, {City} {PostalCode}, {Country}";

    public bool Equals(Address? other)
    {
        if (other is null) return false;
        return Street == other.Street
            && District == other.District
            && City == other.City
            && PostalCode == other.PostalCode
            && Country == other.Country;
    }

    public override bool Equals(object? obj) => obj is Address address && Equals(address);
    public override int GetHashCode() => HashCode.Combine(Street, District, City, PostalCode, Country);
    public override string ToString() => GetFullAddress();

    public static bool operator ==(Address left, Address right) => left.Equals(right);
    public static bool operator !=(Address left, Address right) => !left.Equals(right);
}
