using System.Security.Claims;

namespace HospitalAppointment.API.Extensions;

// API katmanı için claim yardımcı genişletme metotları
public static class ApiServiceExtensions
{
    /// <summary>
    /// Mevcut kullanıcının Identity ID'sini (string) döner.
    /// </summary>
    public static string? GetUserId(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Mevcut kullanıcının e-posta adresini döner.
    /// </summary>
    public static string? GetUserEmail(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Email);

    /// <summary>
    /// Mevcut kullanıcının rolünü döner.
    /// </summary>
    public static string? GetUserRole(this ClaimsPrincipal user)
        => user.FindFirstValue(ClaimTypes.Role);

    /// <summary>
    /// Kullanıcının belirtilen rolde olup olmadığını kontrol eder.
    /// </summary>
    public static bool IsInRole(this ClaimsPrincipal user, string role)
        => user.IsInRole(role);
}
