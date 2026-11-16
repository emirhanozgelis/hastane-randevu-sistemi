using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.ValueObjects;

namespace HospitalAppointment.Domain.Entities.Base;

// Soyut kişi sınıfı — OOP'ta soyutlama (Abstraction) ve kalıtım (Inheritance) için temel
public abstract class Person : BaseEntity
{
    public string FirstName { get; protected set; }
    public string LastName { get; protected set; }
    public string Email { get; protected set; }
    public string PhoneNumber { get; protected set; }
    public DateOnly DateOfBirth { get; protected set; }
    public Gender Gender { get; protected set; }
    public Address? Address { get; protected set; }

    // Hesaplanmış özellik — kapsülleme örneği
    public string FullName => $"{FirstName} {LastName}";
    public int Age => CalculateAge();

    // EF Core materialization için — validation atlanır (sadece ORM kullanır)
    protected Person()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
    }

    protected Person(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateOnly dateOfBirth,
        Gender gender)
    {
        ValidateName(firstName, nameof(firstName));
        ValidateName(lastName, nameof(lastName));
        ValidateEmail(email);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        DateOfBirth = dateOfBirth;
        Gender = gender;
    }

    // Çok biçimlilik (Polymorphism) — alt sınıflar kendi rol adlarını döndürür
    public abstract string GetDisplayRole();

    // İş kuralı entity içinde — kapsülleme
    public void UpdateContactInfo(string email, string phoneNumber)
    {
        ValidateEmail(email);
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        MarkUpdated();
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        MarkUpdated();
    }

    private int CalculateAge()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - DateOfBirth.Year;
        if (DateOfBirth > today.AddYears(-age)) age--;
        return age;
    }

    private static void ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ad/soyad boş olamaz.", paramName);
        if (name.Trim().Length < 2)
            throw new ArgumentException("Ad/soyad en az 2 karakter olmalıdır.", paramName);
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-posta boş olamaz.", nameof(email));
        if (!email.Contains('@'))
            throw new ArgumentException("Geçerli bir e-posta adresi giriniz.", nameof(email));
    }
}
