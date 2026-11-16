using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Web.Models;

// Doğum tarihi validasyonu — tek yerden yönetilir
// Kullanım: [ValidBirthDate] veya [ValidBirthDate(MinAge = 18)]
public class ValidBirthDateAttribute : ValidationAttribute
{
    public int MinAge { get; set; } = 0;       // minimum yaş (doktor için 18 kullanılabilir)
    public int MaxAge { get; set; } = 120;     // maksimum yaş
    public int MinYear { get; set; } = 1900;   // 1900 öncesi kabul edilmez

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not DateOnly date)
            return new ValidationResult("Geçerli bir tarih giriniz.");

        // 0001-01-01 default değerini yakala
        if (date == DateOnly.MinValue || date.Year < MinYear)
            return new ValidationResult($"Doğum tarihi {MinYear} yılından önce olamaz.");

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (date > today)
            return new ValidationResult("Doğum tarihi bugünden ileri bir tarih olamaz.");

        var age = today.Year - date.Year;
        if (date > today.AddYears(-age)) age--;

        if (age > MaxAge)
            return new ValidationResult($"Geçerli bir doğum tarihi giriniz.");

        if (age < MinAge)
            return new ValidationResult($"Bu kayıt için en az {MinAge} yaşında olunması gerekmektedir.");

        return ValidationResult.Success;
    }
}

// İşe giriş tarihi validasyonu
public class ValidHireDateAttribute : ValidationAttribute
{
    public int MinYear { get; set; } = 1950;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not DateOnly date)
            return new ValidationResult("Geçerli bir tarih giriniz.");

        if (date == DateOnly.MinValue || date.Year < MinYear)
            return new ValidationResult($"İşe giriş tarihi {MinYear} yılından önce olamaz.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date > today)
            return new ValidationResult("İşe giriş tarihi gelecekte olamaz.");

        return ValidationResult.Success;
    }
}
