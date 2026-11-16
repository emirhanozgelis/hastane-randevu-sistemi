using Microsoft.AspNetCore.Identity;

namespace HospitalAppointment.Infrastructure.Identity;

// ASP.NET Core Identity kullanıcı sınıfı — ad ve soyad eklenerek genişletildi
public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Hesaplanmış tam ad özelliği
    public string FullName => $"{FirstName} {LastName}";
}
