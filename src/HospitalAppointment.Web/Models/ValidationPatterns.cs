namespace HospitalAppointment.Web.Models;

// Tüm form validasyonları için merkezi regex ve kural sabitleri.
// Yeni kural eklemek veya değiştirmek için yalnızca bu dosyayı düzenlemek yeterlidir.
public static class ValidationPatterns
{
    // Ad / Soyad — Türkçe karakterler açıkça tanımlı (ç,ğ,ı,ö,ş,ü ve büyükleri)
    // \p{L} yerine açık liste: JavaScript (jQuery Validate) \p{L} desteklemez
    public const string Name = @"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s\-']+$";
    public const string NameError = "Yalnızca harf, boşluk, tire veya kesme işareti içerebilir.";
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;

    // Türkiye cep telefonu: 05xxxxxxxxx veya +905xxxxxxxxx
    public const string TurkishPhone = @"^(\+90|0)?5[0-9]{9}$";
    public const string TurkishPhoneError = "Geçerli bir Türk cep numarası girin. Örn: 05321234567";

    // E-posta
    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string EmailError = "Geçerli bir e-posta adresi girin. Örn: ornek@mail.com";

    // Personel/Lisans numarası — harf, rakam, tire, nokta
    public const string Code = @"^[A-Za-z0-9\-\.]+$";
    public const string CodeError = "Yalnızca harf, rakam, tire veya nokta içerebilir.";

    // Uzmanlık / Bölüm adı — harf, rakam, boşluk, tire
    public const string Title = @"^[\p{L}0-9\s\-/]+$";
    public const string TitleError = "Yalnızca harf, rakam, boşluk veya tire içerebilir.";

    // Şifre — en az 6 karakter, en az 1 büyük harf, 1 rakam
    public const string Password = @"^(?=.*[A-Z])(?=.*\d).{6,}$";
    public const string PasswordError = "Şifre en az 6 karakter, 1 büyük harf ve 1 rakam içermelidir.";
    public const int PasswordMinLength = 6;
    public const int PasswordMaxLength = 100;
}
