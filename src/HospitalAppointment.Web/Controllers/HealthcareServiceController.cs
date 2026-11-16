using HospitalAppointment.Application.DTOs.HealthcareService;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAppointment.Web.Controllers;

// Sağlık hizmeti yönetimi kontrolcüsü — Admin veya HospitalManager
[Authorize(Roles = "Admin,HospitalManager")]
public class HealthcareServiceController : Controller
{
    private readonly IHealthcareServiceService _serviceService;
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<HealthcareServiceController> _logger;

    public HealthcareServiceController(
        IHealthcareServiceService serviceService,
        IDepartmentService departmentService,
        ILogger<HealthcareServiceController> logger)
    {
        _serviceService = serviceService;
        _departmentService = departmentService;
        _logger = logger;
    }

    // Hizmet listesi bölüme göre
    public async Task<IActionResult> Index(Guid departmentId, CancellationToken cancellationToken)
    {
        var services = await _serviceService.GetServicesByDepartmentAsync(departmentId, cancellationToken);
        var department = await _departmentService.GetDepartmentAsync(departmentId, cancellationToken);
        ViewBag.DepartmentId = departmentId;
        ViewBag.DepartmentName = department?.Name ?? "Bilinmiyor";
        return View(services);
    }

    // Yeni hizmet oluşturma formu
    [HttpGet]
    public async Task<IActionResult> Create(Guid? departmentId, CancellationToken cancellationToken)
    {
        await PopulateDepartmentsDropdown(departmentId, cancellationToken);
        return View(new CreateHealthcareServiceDto(departmentId ?? Guid.Empty, string.Empty, 30, 0));
    }

    // Yeni hizmet kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHealthcareServiceDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmentsDropdown(dto.DepartmentId, cancellationToken);
            return View(dto);
        }

        try
        {
            await _serviceService.CreateServiceAsync(dto, cancellationToken);
            TempData["Success"] = $"'{dto.Name}' hizmeti başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index), new { departmentId = dto.DepartmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmet oluşturma hatası");
            TempData["Error"] = ex.Message;
        }

        await PopulateDepartmentsDropdown(dto.DepartmentId, cancellationToken);
        return View(dto);
    }

    // Hizmet düzenleme formu
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var service = await _serviceService.GetServiceAsync(id, cancellationToken);
        if (service is null)
            return NotFound();

        var dto = new UpdateHealthcareServiceDto(
            service.Name, service.DurationMinutes, service.Price,
            service.Currency, service.Description);

        ViewBag.ServiceId = id;
        ViewBag.DepartmentId = service.DepartmentId;
        return View(dto);
    }

    // Hizmet bilgilerini kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateHealthcareServiceDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ServiceId = id;
            return View(dto);
        }

        try
        {
            var updated = await _serviceService.UpdateServiceAsync(id, dto, cancellationToken);
            TempData["Success"] = "Hizmet bilgileri güncellendi.";
            return RedirectToAction(nameof(Index), new { departmentId = updated.DepartmentId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hizmet güncelleme hatası: {Id}", id);
            TempData["Error"] = ex.Message;
        }

        ViewBag.ServiceId = id;
        return View(dto);
    }

    private async Task PopulateDepartmentsDropdown(Guid? selectedId, CancellationToken cancellationToken)
    {
        // Bölüm listesi gerçek uygulamada tüm aktif bölümleri içerir
        ViewBag.Departments = new SelectList(
            Enumerable.Empty<object>(), "Id", "Name", selectedId);
    }
}
