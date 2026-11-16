using HospitalAppointment.Application.DTOs.Doctor;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.API.Controllers;

/// <summary>
/// Doktor yönetimi ve müsaitlik endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly ILogger<DoctorsController> _logger;

    public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger)
    {
        _doctorService = doctorService;
        _logger = logger;
    }

    /// <summary>
    /// Doktorları bölüm veya şubeye göre listeler — herkese açık
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetAll(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? branchId,
        CancellationToken cancellationToken)
    {
        if (departmentId.HasValue && departmentId != Guid.Empty)
        {
            var byDept = await _doctorService.GetDoctorsByDepartmentAsync(departmentId.Value, cancellationToken);
            return Ok(byDept);
        }

        if (branchId.HasValue && branchId != Guid.Empty)
        {
            var byBranch = await _doctorService.GetDoctorsByBranchAsync(branchId.Value, cancellationToken);
            return Ok(byBranch);
        }

        return BadRequest(new { error = "departmentId veya branchId parametresi gereklidir." });
    }

    /// <summary>
    /// Müsait doktorları filtreler — herkese açık
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorDto>>> GetAvailable(
        [FromQuery] Guid departmentId,
        [FromQuery] DateOnly date,
        [FromQuery] TimeOnly startTime,
        CancellationToken cancellationToken)
    {
        var doctors = await _doctorService.GetAvailableDoctorsAsync(departmentId, date, startTime, cancellationToken);
        return Ok(doctors);
    }

    /// <summary>
    /// Belirli bir doktoru getirir — herkese açık
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorAsync(id, cancellationToken);
        if (doctor is null)
            return NotFound(new { error = "Doktor bulunamadı." });

        return Ok(doctor);
    }

    /// <summary>
    /// Doktorun müsaitlik takvimini getirir — herkese açık
    /// </summary>
    [HttpGet("{id:guid}/availability")]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DoctorAvailabilityDto>>> GetAvailability(
        Guid id,
        CancellationToken cancellationToken)
    {
        var availability = await _doctorService.GetDoctorAvailabilityAsync(id, cancellationToken);
        return Ok(availability);
    }

    /// <summary>
    /// Yeni doktor oluşturur — Admin veya HospitalManager
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorDto>> Create(
        [FromBody] CreateDoctorDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni doktor oluşturuluyor: {Email}", dto.Email);
        var doctor = await _doctorService.CreateDoctorAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
    }

    /// <summary>
    /// Doktora müsaitlik ekler — Admin veya Doctor
    /// </summary>
    [HttpPost("{id:guid}/availability")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(typeof(DoctorAvailabilityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorAvailabilityDto>> AddAvailability(
        Guid id,
        [FromBody] CreateDoctorAvailabilityDto dto,
        CancellationToken cancellationToken)
    {
        // DTO'daki DoctorId ile URL'deki id eşleşmeli
        if (dto.DoctorId != id)
            return BadRequest(new { error = "URL'deki doktor ID ile gövdedeki DoctorId eşleşmiyor." });

        _logger.LogInformation("Doktor müsaitliği ekleniyor: DoctorId={Id}", id);
        var availability = await _doctorService.AddAvailabilityAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetAvailability), new { id }, availability);
    }

    /// <summary>
    /// Doktor müsaitlik kaydını siler — Admin veya Doctor
    /// </summary>
    [HttpDelete("availability/{availabilityId:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAvailability(Guid availabilityId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Doktor müsaitliği siliniyor: {AvailabilityId}", availabilityId);
        await _doctorService.DeleteAvailabilityAsync(availabilityId, cancellationToken);
        return NoContent();
    }
}
