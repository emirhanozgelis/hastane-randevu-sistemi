using HospitalAppointment.Application.DTOs.Department;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.API.Controllers;

/// <summary>
/// Bölüm yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DepartmentsController> _logger;

    public DepartmentsController(IDepartmentService departmentService, ILogger<DepartmentsController> logger)
    {
        _departmentService = departmentService;
        _logger = logger;
    }

    /// <summary>
    /// Şubeye ait bölümleri listeler — herkese açık
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetByBranch(
        [FromQuery] Guid branchId,
        CancellationToken cancellationToken)
    {
        if (branchId == Guid.Empty)
            return BadRequest(new { error = "branchId parametresi zorunludur." });

        var departments = await _departmentService.GetDepartmentsByBranchAsync(branchId, cancellationToken);
        return Ok(departments);
    }

    /// <summary>
    /// Belirli bir bölümü getirir — herkese açık
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetDepartmentAsync(id, cancellationToken);
        if (department is null)
            return NotFound(new { error = "Bölüm bulunamadı." });

        return Ok(department);
    }

    /// <summary>
    /// Yeni bölüm oluşturur — Admin veya HospitalManager
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DepartmentDto>> Create(
        [FromBody] CreateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni bölüm oluşturuluyor: {Name}", dto.Name);
        var department = await _departmentService.CreateDepartmentAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    /// <summary>
    /// Bölüm bilgilerini günceller — Admin veya HospitalManager
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DepartmentDto>> Update(
        Guid id,
        [FromBody] UpdateDepartmentDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Bölüm güncelleniyor: {Id}", id);
        var department = await _departmentService.UpdateDepartmentAsync(id, dto, cancellationToken);
        return Ok(department);
    }
}
