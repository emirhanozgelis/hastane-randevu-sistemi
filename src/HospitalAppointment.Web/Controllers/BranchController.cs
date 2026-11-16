using HospitalAppointment.Application.DTOs.Branch;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAppointment.Web.Controllers;

// Şube yönetimi kontrolcüsü — Admin veya HospitalManager
[Authorize(Roles = "Admin,HospitalManager")]
public class BranchController : Controller
{
    private readonly IBranchService _branchService;
    private readonly IHospitalService _hospitalService;
    private readonly ILogger<BranchController> _logger;

    public BranchController(
        IBranchService branchService,
        IHospitalService hospitalService,
        ILogger<BranchController> logger)
    {
        _branchService = branchService;
        _hospitalService = hospitalService;
        _logger = logger;
    }

    // Şube listesi — hospitalId verilmezse tüm hastanelerin şubeleri gösterilir
    public async Task<IActionResult> Index(Guid? hospitalId, CancellationToken cancellationToken)
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);

        if (hospitalId.HasValue && hospitalId != Guid.Empty)
        {
            var branches = await _branchService.GetBranchesByHospitalAsync(hospitalId.Value, cancellationToken);
            var hospital = hospitals.FirstOrDefault(h => h.Id == hospitalId.Value);
            ViewBag.HospitalId = hospitalId;
            ViewBag.HospitalName = hospital?.Name ?? "Bilinmiyor";
            ViewBag.Hospitals = hospitals;
            return View(branches);
        }

        // Hastane seçilmemişse: tüm hastanelerin şubelerini topla
        var allBranches = new List<Application.DTOs.Branch.BranchDto>();
        foreach (var h in hospitals)
        {
            var hBranches = await _branchService.GetBranchesByHospitalAsync(h.Id, cancellationToken);
            allBranches.AddRange(hBranches);
        }

        ViewBag.HospitalId = Guid.Empty;
        ViewBag.HospitalName = "Tüm Hastaneler";
        ViewBag.Hospitals = hospitals;
        return View(allBranches.AsReadOnly());
    }

    // Yeni şube oluşturma formu
    [HttpGet]
    public async Task<IActionResult> Create(Guid? hospitalId, CancellationToken cancellationToken)
    {
        await PopulateHospitalsDropdown(hospitalId, cancellationToken);
        return View(new CreateBranchDto(
            hospitalId ?? Guid.Empty,
            string.Empty, string.Empty, string.Empty,
            string.Empty, string.Empty, string.Empty));
    }

    // Yeni şube kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBranchDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateHospitalsDropdown(dto.HospitalId, cancellationToken);
            return View(dto);
        }

        try
        {
            await _branchService.CreateBranchAsync(dto, cancellationToken);
            TempData["Success"] = $"'{dto.Name}' şubesi başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index), new { hospitalId = dto.HospitalId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şube oluşturma hatası");
            TempData["Error"] = ex.Message;
        }

        await PopulateHospitalsDropdown(dto.HospitalId, cancellationToken);
        return View(dto);
    }

    // Şube düzenleme formu
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var branch = await _branchService.GetBranchAsync(id, cancellationToken);
        if (branch is null)
            return NotFound();

        var dto = new UpdateBranchDto(
            branch.Name, string.Empty, string.Empty,
            string.Empty, string.Empty, branch.PhoneNumber, branch.Email);

        ViewBag.BranchId = id;
        ViewBag.HospitalId = branch.HospitalId;
        return View(dto);
    }

    // Şube bilgilerini kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateBranchDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.BranchId = id;
            return View(dto);
        }

        try
        {
            var updated = await _branchService.UpdateBranchAsync(id, dto, cancellationToken);
            TempData["Success"] = "Şube bilgileri güncellendi.";
            return RedirectToAction(nameof(Index), new { hospitalId = updated.HospitalId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Şube güncelleme hatası: {Id}", id);
            TempData["Error"] = ex.Message;
        }

        ViewBag.BranchId = id;
        return View(dto);
    }

    private async Task PopulateHospitalsDropdown(Guid? selectedId, CancellationToken cancellationToken)
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);
        ViewBag.Hospitals = new SelectList(hospitals, "Id", "Name", selectedId);
    }
}
