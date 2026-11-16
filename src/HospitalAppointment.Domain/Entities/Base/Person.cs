using System.Text.RegularExpressions;
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
        ValidatePhone(phoneNumber);
        ValidateDateOfBirth(dateOfBirth);

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PhoneNumber = NormalizePhone(phoneNumber);
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

    // Backend'de de doğum tarihi koruması — 0001-01-01 ve 1900 öncesi kabul edilmez
    private static void ValidateDateOfBirth(DateOnly date)
    {
        if (date == DateOnly.MinValue || date.Year < 1900)
            throw new ArgumentException("Doğum tarihi 1900 yılından önce olamaz.", nameof(date));
        if (date > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Doğum tarihi bugünden ileri bir tarih olamaz.", nameof(date));
    }

    private static void ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ad/soyad boş olamaz.", paramName);
        if (name.Trim().Length < 2)
            throw new ArgumentException("Ad/soyad en az 2 karakter olmalıdır.", paramName);
        if (!Regex.IsMatch(name.Trim(), @"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s\-']+$"))
            throw new ArgumentException("Ad/soyad yalnızca harf, boşluk, tire veya kesme işareti içerebilir.", paramName);
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-posta boş olamaz.", nameof(email));
        if (!Regex.IsMatch(email.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Geçerli bir e-posta adresi giriniz. Örn: ornek@mail.com", nameof(email));
    }

    // Telefon formatı: +90 (5xx) xxx xx xx — formdan gelen format veya rakam dizisi
    private static void ValidatePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return;
        // Hem formatlanmış (+90 (5xx) xxx xx xx) hem ham (05xxxxxxxxx) kabul edilir
        var stripped = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
        if (!Regex.IsMatch(stripped, @"^(90)?5[0-9]{9}$"))
            throw new ArgumentException("Geçerli bir Türk telefon numarası giriniz.", nameof(phone));
    }

    // Telefonu +90 (5xx) xxx xx xx formatına normalize eder
    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = Regex.Replace(phone, @"[^\d]", "");
        // Başındaki 0 veya 90 kaldır, 10 haneli core numarayı al
        if (digits.StartsWith("90") && digits.Length == 12) digits = digits[2..];
        else if (digits.StartsWith("0") && digits.Length == 11) digits = digits[1..];
        if (digits.Length == 10)
            return $"+90 ({digits[..3]}) {digits[3..6]} {digits[6..8]} {digits[8..10]}";
        return phone.Trim(); // format dışıysa olduğu gibi sakla
    }
}
