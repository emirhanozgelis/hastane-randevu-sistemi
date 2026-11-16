using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HospitalAppointment.Application.DTOs.Auth;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HospitalAppointment.Infrastructure.Identity;

// Kimlik doğrulama servis implementasyonu — JWT token üretimi ve kullanıcı yönetimi
public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    // Hasta kaydı: kullanıcı oluştur, rol ata, hasta entity kaydet
    public async Task<AuthResponseDto> RegisterPatientAsync(
        RegisterDto dto,
        CancellationToken cancellationToken = default)
    {
        // Mevcut e-posta kontrolü
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
            throw new InvalidOperationException($"Bu e-posta adresi zaten kayıtlı: {dto.Email}");

        // Identity kullanıcısı oluştur
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PhoneNumber = dto.PhoneNumber
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Kullanıcı oluşturulamadı: {errors}");
        }

        // Patient rolü yoksa oluştur
        if (!await _roleManager.RoleExistsAsync("Patient"))
            await _roleManager.CreateAsync(new IdentityRole("Patient"));

        await _userManager.AddToRoleAsync(user, "Patient");

        // Hasta domain entity oluştur
        var patient = new Patient(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.Gender,
            user.Id,
            dto.BloodType);

        await _patientRepository.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // JWT token üret ve yanıt döndür
        var token = await GenerateJwtTokenAsync(user);
        return new AuthResponseDto(
            user.Id,
            user.Email!,
            user.FullName,
            "Patient",
            token.Token,
            token.ExpiresAt);
    }

    // Giriş: kimlik doğrula ve JWT token üret
    public async Task<AuthResponseDto> LoginAsync(
        LoginDto dto,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email)
            ?? throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
            throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient";

        var token = await GenerateJwtTokenAsync(user);
        return new AuthResponseDto(
            user.Id,
            user.Email!,
            user.FullName,
            role,
            token.Token,
            token.ExpiresAt);
    }

    // Çıkış — Identity için özel işlem gerekmez (JWT stateless)
    public async Task LogoutAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _signInManager.SignOutAsync();
    }

    // Şifre değiştir
    public async Task<bool> ChangePasswordAsync(
        string userId,
        ChangePasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new InvalidOperationException("Yeni şifreler eşleşmiyor.");

        var user = await _userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Kullanıcı bulunamadı.");

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        return result.Succeeded;
    }

    // Kullanıcının ilk rolünü getir
    public async Task<string?> GetUserRoleAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault();
    }

    // Çalışan (doktor/personel) kullanıcı hesabı oluşturur — hasta domain entity'si oluşturmaz
    public async Task<string> CreateEmployeeUserAsync(
        string email,
        string firstName,
        string lastName,
        string password,
        string role,
        CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
            throw new InvalidOperationException($"Bu e-posta adresi zaten kayıtlı: {email}");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Kullanıcı oluşturulamadı: {errors}");
        }

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        await _userManager.AddToRoleAsync(user, role);
        return user.Id;
    }

    // JWT token oluşturma — HMAC-SHA256 imzalı, 8 saat geçerli
    private async Task<(string Token, DateTime ExpiresAt)> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? "Patient";

        var expiresAt = DateTime.UtcNow.AddHours(8);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role)
        };

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT anahtarı yapılandırılmamış.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var issuer = _configuration["Jwt:Issuer"] ?? "HospitalAppointment";
        var audience = _configuration["Jwt:Audience"] ?? "HospitalAppointment";

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return (tokenString, expiresAt);
    }
}
