using HospitalAppointment.Application.DTOs.Hospital;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.API.Controllers;

/// <summary>
/// Hastane yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HospitalsController : ControllerBase
{
    private readonly IHospitalService _hospitalService;
    private readonly ILogger<HospitalsController> _logger;

    public HospitalsController(IHospitalService hospitalService, ILogger<HospitalsController> logger)
    {
        _hospitalService = hospitalService;
        _logger = logger;
    }

    /// <summary>
    /// Tüm hastaneleri listeler — herkese açık
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HospitalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HospitalDto>>> GetAll(CancellationToken cancellationToken)
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);
        return Ok(hospitals);
    }

    /// <summary>
    /// Belirli bir hastane getirir — herkese açık
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HospitalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HospitalDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var hospital = await _hospitalService.GetHospitalAsync(id, cancellationToken);
        if (hospital is null)
            return NotFound(new { error = "Hastane bulunamadı." });

        return Ok(hospital);
    }

    /// <summary>
    /// Yeni hastane oluşturur — sadece Admin
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(HospitalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<HospitalDto>> Create(
        [FromBody] CreateHospitalDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni hastane oluşturuluyor: {Name}", dto.Name);
        var hospital = await _hospitalService.CreateHospitalAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = hospital.Id }, hospital);
    }

    /// <summary>
    /// Hastane bilgilerini günceller — sadece Admin
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(HospitalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HospitalDto>> Update(
        Guid id,
        [FromBody] UpdateHospitalDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hastane güncelleniyor: {Id}", id);
        var hospital = await _hospitalService.UpdateHospitalAsync(id, dto, cancellationToken);
        return Ok(hospital);
    }

    /// <summary>
    /// Hastaneyi pasife alır — sadece Admin
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Hastane pasife alınıyor: {Id}", id);
        await _hospitalService.DeactivateHospitalAsync(id, cancellationToken);
        return NoContent();
    }
}
