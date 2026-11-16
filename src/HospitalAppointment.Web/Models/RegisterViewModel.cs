using System.ComponentModel.DataAnnotations;
using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Web.Models;

// Hasta kayıt formu view model'i — tüm regex kuralları ValidationPatterns'den gelir
public class RegisterViewModel
{
    [Required(ErrorMessage = "Ad zorunludur.")]
    [StringLength(ValidationPatterns.NameMaxLength, MinimumLength = ValidationPatterns.NameMinLength,
        ErrorMessage = "Ad 2-50 karakter arasında olmalıdır.")]
    [RegularExpression(ValidationPatterns.Name, ErrorMessage = "Ad " + ValidationPatterns.NameError)]
    [Display(Name = "Ad")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [StringLength(ValidationPatterns.NameMaxLength, MinimumLength = ValidationPatterns.NameMinLength,
        ErrorMessage = "Soyad 2-50 karakter arasında olmalıdır.")]
    [RegularExpression(ValidationPatterns.Name, ErrorMessage = "Soyad " + ValidationPatterns.NameError)]
    [Display(Name = "Soyad")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur.")]
    [RegularExpression(ValidationPatterns.Email, ErrorMessage = ValidationPatterns.EmailError)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(ValidationPatterns.PasswordMaxLength, MinimumLength = ValidationPatterns.PasswordMinLength,
        ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Şifreler eşleşmiyor.")]
    [Display(Name = "Şifre Tekrarı")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [RegularExpression(ValidationPatterns.TurkishPhone, ErrorMessage = ValidationPatterns.TurkishPhoneError)]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; }

    [Required(ErrorMessage = "Cinsiyet zorunludur.")]
    [Display(Name = "Cinsiyet")]
    public Gender Gender { get; set; }

    [Display(Name = "Kan Grubu")]
    public BloodType BloodType { get; set; } = BloodType.Unknown;
}
