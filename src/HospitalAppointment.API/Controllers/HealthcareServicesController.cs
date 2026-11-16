using HospitalAppointment.Application.DTOs.HealthcareService;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.API.Controllers;

/// <summary>
/// Sağlık hizmeti yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/services")]
public class HealthcareServicesController : ControllerBase
{
    private readonly IHealthcareServiceService _serviceService;
    private readonly ILogger<HealthcareServicesController> _logger;

    public HealthcareServicesController(
        IHealthcareServiceService serviceService,
        ILogger<HealthcareServicesController> logger)
    {
        _serviceService = serviceService;
        _logger = logger;
    }

    /// <summary>
    /// Bölüme ait sağlık hizmetlerini listeler — herkese açık
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HealthcareServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HealthcareServiceDto>>> GetByDepartment(
        [FromQuery] Guid departmentId,
        CancellationToken cancellationToken)
    {
        if (departmentId == Guid.Empty)
            return BadRequest(new { error = "departmentId parametresi zorunludur." });

        var services = await _serviceService.GetServicesByDepartmentAsync(departmentId, cancellationToken);
        return Ok(services);
    }

    /// <summary>
    /// Belirli bir hizmeti getirir — herkese açık
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HealthcareServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthcareServiceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var service = await _serviceService.GetServiceAsync(id, cancellationToken);
        if (service is null)
            return NotFound(new { error = "Sağlık hizmeti bulunamadı." });

        return Ok(service);
    }

    /// <summary>
    /// Yeni sağlık hizmeti oluşturur — Admin veya HospitalManager
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(HealthcareServiceDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<HealthcareServiceDto>> Create(
        [FromBody] CreateHealthcareServiceDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni sağlık hizmeti oluşturuluyor: {Name}", dto.Name);
        var service = await _serviceService.CreateServiceAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, service);
    }

    /// <summary>
    /// Sağlık hizmetini günceller — Admin veya HospitalManager
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(HealthcareServiceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HealthcareServiceDto>> Update(
        Guid id,
        [FromBody] UpdateHealthcareServiceDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sağlık hizmeti güncelleniyor: {Id}", id);
        var service = await _serviceService.UpdateServiceAsync(id, dto, cancellationToken);
        return Ok(service);
    }

    /// <summary>
    /// Sağlık hizmetini pasife alır — Admin veya HospitalManager
    /// </summary>
    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _serviceService.DeactivateServiceAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Sağlık hizmetini aktife alır — Admin veya HospitalManager
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await _serviceService.ActivateServiceAsync(id, cancellationToken);
        return NoContent();
    }
}
