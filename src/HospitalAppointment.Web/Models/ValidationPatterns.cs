namespace HospitalAppointment.Web.Models;

// Tüm form validasyonları için merkezi regex ve kural sabitleri.
// Değişiklik için yalnızca bu dosyayı düzenlemek yeterlidir.
public static class ValidationPatterns
{
    // Ad / Soyad — Türkçe karakterler açıkça tanımlı (jQuery Validate \p{L} desteklemez)
    public const string Name = @"^[a-zA-ZçÇğĞıİöÖşŞüÜ\s\-']+$";
    public const string NameError = "Yalnızca harf, boşluk, tire veya kesme işareti içerebilir.";
    public const int NameMinLength = 2;
    public const int NameMaxLength = 50;

    // Türkiye telefonu — +90 (5xx) xxx xx xx formatında saklanır
    // Kullanıcı girerken mask JS ile biçimlendirilir, kayıt öncesi temizlenir
    public const string TurkishPhone = @"^\+90 \([5][0-9]{2}\) [0-9]{3} [0-9]{2} [0-9]{2}$";
    public const string TurkishPhoneError = "Lütfen +90 (5xx) xxx xx xx formatında girin.";
    public const string TurkishPhonePlaceholder = "+90 (5__) ___ __ __";

    // E-posta
    public const string Email = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
    public const string EmailError = "Geçerli bir e-posta adresi girin. Örn: ornek@mail.com";

    // Personel / Lisans numarası — alfanumerik, tire, nokta
    public const string Code = @"^[A-Za-z0-9\-\.]+$";
    public const string CodeError = "Yalnızca harf, rakam, tire veya nokta içerebilir.";

    // Uzmanlık / Bölüm / Başlık — Türkçe harfler, rakam, boşluk, tire, /
    public const string Title = @"^[a-zA-ZçÇğĞıİöÖşŞüÜ0-9\s\-/()]+$";
    public const string TitleError = "Yalnızca harf, rakam, boşluk veya tire içerebilir.";

    // Şifre — en az 6 karakter, 1 büyük harf, 1 rakam
    public const string Password = @"^(?=.*[A-Z])(?=.*\d).{6,}$";
    public const string PasswordError = "Şifre en az 6 karakter, 1 büyük harf ve 1 rakam içermelidir.";
    public const int PasswordMinLength = 6;
    public const int PasswordMaxLength = 100;

    // Türkiye'deki tıbbi uzmanlık alanları — Sağlık Bakanlığı tabanlı liste
    public static readonly IReadOnlyList<string> MedicalSpecializations = new[]
    {
        "Acil Tıp",
        "Aile Hekimliği",
        "Anesteziyoloji ve Reanimasyon",
        "Beyin ve Sinir Cerrahisi",
        "Çocuk Cerrahisi",
        "Çocuk Sağlığı ve Hastalıkları (Pediatri)",
        "Deri ve Zührevi Hastalıkları (Dermatoloji)",
        "Endokrinoloji ve Metabolizma Hastalıkları",
        "Enfeksiyon Hastalıkları ve Klinik Mikrobiyoloji",
        "Fiziksel Tıp ve Rehabilitasyon",
        "Gastroenteroloji",
        "Genel Cerrahi",
        "Göğüs Cerrahisi",
        "Göğüs Hastalıkları (Pulmonoloji)",
        "Göz Hastalıkları (Oftalmoloji)",
        "Hematoloji",
        "İç Hastalıkları (Dahiliye)",
        "Kadın Hastalıkları ve Doğum (Jinekoloji)",
        "Kalp ve Damar Cerrahisi",
        "Kardiyoloji",
        "Kulak Burun Boğaz (KBB)",
        "Nefroloji",
        "Nöroloji",
        "Nöroşirürji",
        "Nükleer Tıp",
        "Onkoloji (Tıbbi Onkoloji)",
        "Ortopedi ve Travmatoloji",
        "Patoloji",
        "Plastik, Rekonstrüktif ve Estetik Cerrahi",
        "Psikiyatri",
        "Radyasyon Onkolojisi",
        "Radyoloji",
        "Romatoloji",
        "Spor Hekimliği",
        "Üroloji",
    };
}
