using HospitalAppointment.Application.DTOs.Branch;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.API.Controllers;

/// <summary>
/// Şube yönetimi endpoint'leri
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IBranchService _branchService;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(IBranchService branchService, ILogger<BranchesController> logger)
    {
        _branchService = branchService;
        _logger = logger;
    }

    /// <summary>
    /// Hastaneye ait şubeleri listeler — herkese açık
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BranchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<BranchDto>>> GetByHospital(
        [FromQuery] Guid hospitalId,
        CancellationToken cancellationToken)
    {
        if (hospitalId == Guid.Empty)
            return BadRequest(new { error = "hospitalId parametresi zorunludur." });

        var branches = await _branchService.GetBranchesByHospitalAsync(hospitalId, cancellationToken);
        return Ok(branches);
    }

    /// <summary>
    /// Belirli bir şubeyi getirir — herkese açık
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BranchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BranchDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetBranchAsync(id, cancellationToken);
        if (branch is null)
            return NotFound(new { error = "Şube bulunamadı." });

        return Ok(branch);
    }

    /// <summary>
    /// Yeni şube oluşturur — Admin veya HospitalManager
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(BranchDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BranchDto>> Create(
        [FromBody] CreateBranchDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Yeni şube oluşturuluyor: {Name}", dto.Name);
        var branch = await _branchService.CreateBranchAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = branch.Id }, branch);
    }

    /// <summary>
    /// Şube bilgilerini günceller — Admin veya HospitalManager
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ProducesResponseType(typeof(BranchDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BranchDto>> Update(
        Guid id,
        [FromBody] UpdateBranchDto dto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Şube güncelleniyor: {Id}", id);
        var branch = await _branchService.UpdateBranchAsync(id, dto, cancellationToken);
        return Ok(branch);
    }
}
