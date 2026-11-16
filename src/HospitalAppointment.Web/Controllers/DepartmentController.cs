using HospitalAppointment.Application.DTOs.Department;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAppointment.Web.Controllers;

// Bölüm yönetimi kontrolcüsü — Admin veya HospitalManager
[Authorize(Roles = "Admin,HospitalManager")]
public class DepartmentController : Controller
{
    private readonly IDepartmentService _departmentService;
    private readonly IBranchService _branchService;
    private readonly ILogger<DepartmentController> _logger;

    public DepartmentController(
        IDepartmentService departmentService,
        IBranchService branchService,
        ILogger<DepartmentController> logger)
    {
        _departmentService = departmentService;
        _branchService = branchService;
        _logger = logger;
    }

    // Bölüm listesi — branchId verilmezse yönlendir
    public async Task<IActionResult> Index(Guid? branchId, CancellationToken cancellationToken)
    {
        if (!branchId.HasValue || branchId == Guid.Empty)
        {
            TempData["Error"] = "Bölümleri görüntülemek için önce bir şube seçin.";
            return RedirectToAction("Index", "Branch");
        }

        var departments = await _departmentService.GetDepartmentsByBranchAsync(branchId.Value, cancellationToken);
        var branch = await _branchService.GetBranchAsync(branchId.Value, cancellationToken);
        ViewBag.BranchId = branchId;
        ViewBag.BranchName = branch?.Name ?? "Bilinmiyor";
        return View(departments);
    }

    // Yeni bölüm oluşturma formu
    [HttpGet]
    public async Task<IActionResult> Create(Guid? branchId, CancellationToken cancellationToken)
    {
        await PopulateBranchesDropdown(branchId, cancellationToken);
        return View(new CreateDepartmentDto(branchId ?? Guid.Empty, string.Empty));
    }

    // Yeni bölüm kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDepartmentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateBranchesDropdown(dto.BranchId, cancellationToken);
            return View(dto);
        }

        try
        {
            await _departmentService.CreateDepartmentAsync(dto, cancellationToken);
            TempData["Success"] = $"'{dto.Name}' bölümü başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index), new { branchId = dto.BranchId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölüm oluşturma hatası");
            TempData["Error"] = ex.Message;
        }

        await PopulateBranchesDropdown(dto.BranchId, cancellationToken);
        return View(dto);
    }

    // Bölüm düzenleme formu
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var department = await _departmentService.GetDepartmentAsync(id, cancellationToken);
        if (department is null)
            return NotFound();

        var dto = new UpdateDepartmentDto(department.Name, department.Description);
        ViewBag.DepartmentId = id;
        ViewBag.BranchId = department.BranchId;
        return View(dto);
    }

    // Bölüm bilgilerini kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateDepartmentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DepartmentId = id;
            return View(dto);
        }

        try
        {
            var updated = await _departmentService.UpdateDepartmentAsync(id, dto, cancellationToken);
            TempData["Success"] = "Bölüm bilgileri güncellendi.";
            return RedirectToAction(nameof(Index), new { branchId = updated.BranchId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bölüm güncelleme hatası: {Id}", id);
            TempData["Error"] = ex.Message;
        }

        ViewBag.DepartmentId = id;
        return View(dto);
    }

    private async Task PopulateBranchesDropdown(Guid? selectedId, CancellationToken cancellationToken)
    {
        // Şubeleri çekmek için tüm şubelere erişim gerekir — hospitalId olmadan
        // Bu örnekte ViewBag üzerinden boş liste verilir; gerçek uygulamada
        // HospitalService üzerinden tüm şubeler yüklenebilir.
        ViewBag.Branches = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
            Enumerable.Empty<object>(), "Id", "Name", selectedId);
    }
}
