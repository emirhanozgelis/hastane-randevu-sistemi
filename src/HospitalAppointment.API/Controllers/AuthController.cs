using HospitalAppointment.Application.DTOs.Auth;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.API.Controllers;

/// <summary>
/// Kimlik doğrulama işlemleri — kayıt ve giriş
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Yeni hasta kaydı oluşturur
    /// </summary>
    /// <param name="dto">Kayıt bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>JWT token ve kullanıcı bilgileri</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponseDto>> Register(
        [FromBody] RegisterDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni hasta kaydı: {Email}", dto.Email);
        var result = await _authService.RegisterPatientAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Kullanıcı girişi yapar ve JWT token döner
    /// </summary>
    /// <param name="dto">Giriş bilgileri</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>JWT token ve kullanıcı bilgileri</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Giriş denemesi: {Email}", dto.Email);
        var result = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(result);
    }
}
