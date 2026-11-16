using HospitalAppointment.Application.DTOs.Auth;

namespace HospitalAppointment.Application.Interfaces;

// Kimlik doğrulama servis arayüzü — OCP gereği yeni auth yöntemleri için genişletilebilir
public interface IAuthService
{
    Task<AuthResponseDto> RegisterPatientAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(string userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task<string?> GetUserRoleAsync(string userId, CancellationToken cancellationToken = default);

    // Çalışan (doktor/personel) kullanıcı hesabı oluşturur — hasta kaydından farklı olarak domain entity oluşturmaz
    Task<string> CreateEmployeeUserAsync(string email, string firstName, string lastName, string password, string role, CancellationToken cancellationToken = default);
}
