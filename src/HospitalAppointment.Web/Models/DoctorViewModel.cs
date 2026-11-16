using System.ComponentModel.DataAnnotations;
using HospitalAppointment.Domain.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAppointment.Web.Models;

// Doktor oluşturma formu view model'i — ValidationPatterns'den merkezi kurallar
public class CreateDoctorViewModel
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

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [RegularExpression(ValidationPatterns.TurkishPhone, ErrorMessage = ValidationPatterns.TurkishPhoneError)]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    [ValidBirthDate(MinAge = 18, MaxAge = 80, MinYear = 1900,
        ErrorMessage = "Doğum tarihi geçersiz. Doktor en az 18 yaşında olmalıdır.")]
    [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateOnly DateOfBirth { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(-30));

    [Required(ErrorMessage = "Cinsiyet zorunludur.")]
    [Display(Name = "Cinsiyet")]
    public Gender Gender { get; set; }

    [Required(ErrorMessage = "Personel numarası zorunludur.")]
    [RegularExpression(ValidationPatterns.Code, ErrorMessage = "Personel no: " + ValidationPatterns.CodeError)]
    [Display(Name = "Personel Numarası")]
    public string EmployeeNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "İşe giriş tarihi zorunludur.")]
    [ValidHireDate(MinYear = 1950,
        ErrorMessage = "İşe giriş tarihi geçersiz (1950 öncesi veya gelecek tarih olamaz).")]
    [Display(Name = "İşe Giriş Tarihi")]
    [DataType(DataType.Date)]
    public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "Şube seçimi zorunludur.")]
    [Display(Name = "Şube")]
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "Bölüm seçimi zorunludur.")]
    [Display(Name = "Bölüm")]
    public Guid DepartmentId { get; set; }

    // Dropdown — ValidationPatterns.MedicalSpecializations listesinden seçilir
    [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
    [Display(Name = "Uzmanlık Alanı")]
    public string Specialization { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lisans numarası zorunludur.")]
    [RegularExpression(ValidationPatterns.Code, ErrorMessage = "Lisans no: " + ValidationPatterns.CodeError)]
    [Display(Name = "Lisans Numarası")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [StringLength(ValidationPatterns.PasswordMaxLength, MinimumLength = ValidationPatterns.PasswordMinLength,
        ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    [DataType(DataType.Password)]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Biyografi")]
    public string? Biography { get; set; }

    // Dropdown listeler
    public SelectList? Branches { get; set; }
    public SelectList? Departments { get; set; }
}
