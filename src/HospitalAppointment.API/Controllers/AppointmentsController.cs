using System.Security.Claims;
using HospitalAppointment.Application.DTOs.Appointment;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.API.Controllers;

/// <summary>
/// Randevu yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<AppointmentsController> _logger;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientRepository patientRepository,
        ILogger<AppointmentsController> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    // ─── Listeleme ────────────────────────────────────────────────────────────

    /// <summary>
    /// Tüm randevuları listeler — Admin veya HospitalManager
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetAll(
        [FromQuery] Guid? branchId,
        CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetAllAppointmentsAsync(branchId, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Hastanın kendi randevularını getirir — Patient rolü
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetMyAppointments(
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var patient = await _patientRepository.GetByUserIdAsync(userId, cancellationToken);
        if (patient is null)
            return NotFound(new { error = "Hasta kaydı bulunamadı." });

        var appointments = await _appointmentService.GetPatientAppointmentsAsync(patient.Id, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Doktorun kendi randevularını getirir — Doctor rolü
    /// </summary>
    [HttpGet("doctor")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> GetDoctorAppointments(
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var doctor = await _doctorService.GetDoctorByUserIdAsync(userId, cancellationToken);
        if (doctor is null)
            return NotFound(new { error = "Doktor kaydı bulunamadı." });

        var appointments = await _appointmentService.GetDoctorAppointmentsAsync(doctor.Id, date, cancellationToken);
        return Ok(appointments);
    }

    /// <summary>
    /// Belirli bir randevuyu getirir — kimlik doğrulaması gerekli
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetAppointmentAsync(id, cancellationToken);
        if (appointment is null)
            return NotFound(new { error = "Randevu bulunamadı." });

        return Ok(appointment);
    }

    // ─── Oluşturma ────────────────────────────────────────────────────────────

    /// <summary>
    /// Yeni randevu oluşturur — Patient rolü
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromBody] CreateAppointmentDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var patient = await _patientRepository.GetByUserIdAsync(userId, cancellationToken);
        if (patient is null)
            return NotFound(new { error = "Hasta kaydı bulunamadı." });

        _logger.LogInformation("Randevu oluşturuluyor: PatientId={PatientId}", patient.Id);
        var appointment = await _appointmentService.CreateAppointmentAsync(patient.Id, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    // ─── Durum Geçişleri ──────────────────────────────────────────────────────

    /// <summary>
    /// Randevuyu onaylar — Admin, HospitalManager veya Doctor
    /// </summary>
    [HttpPost("{id:guid}/confirm")]
    [Authorize(Roles = "Admin,HospitalManager,Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        Guid id,
        [FromBody] UpdateAppointmentStatusDto? dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await _appointmentService.ConfirmAppointmentAsync(id, userId, dto?.Notes, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Randevuyu reddeder — Admin, HospitalManager veya Doctor
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Admin,HospitalManager,Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody] UpdateAppointmentStatusDto dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        var reason = dto?.Notes ?? "Belirtilmedi";
        await _appointmentService.RejectAppointmentAsync(id, userId, reason, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Randevuyu iptal eder — Admin, HospitalManager, Doctor veya Patient
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,HospitalManager,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(
        Guid id,
        [FromBody] UpdateAppointmentStatusDto? dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await _appointmentService.CancelAppointmentAsync(id, userId, dto?.Notes, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Randevuyu tamamlandı olarak işaretler — Admin veya Doctor
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(
        Guid id,
        [FromBody] UpdateAppointmentStatusDto? dto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await _appointmentService.CompleteAppointmentAsync(id, userId, dto?.Notes, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Randevuyu gelmeyen hasta olarak işaretler — Admin veya Doctor
    /// </summary>
    [HttpPost("{id:guid}/noshow")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> NoShow(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        await _appointmentService.MarkAsNoShowAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
