using HospitalAppointment.Application.DTOs.Hospital;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Web.Controllers;

// Hastane yönetimi kontrolcüsü — sadece Admin
[Authorize(Roles = "Admin")]
public class HospitalController : Controller
{
    private readonly IHospitalService _hospitalService;
    private readonly ILogger<HospitalController> _logger;

    public HospitalController(IHospitalService hospitalService, ILogger<HospitalController> logger)
    {
        _hospitalService = hospitalService;
        _logger = logger;
    }

    // Hastane listesi
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);
        return View(hospitals);
    }

    // Yeni hastane oluşturma formu
    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateHospitalDto(
            string.Empty, string.Empty, string.Empty,
            string.Empty, string.Empty));
    }

    // Yeni hastane kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateHospitalDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            await _hospitalService.CreateHospitalAsync(dto, cancellationToken);
            TempData["Success"] = $"'{dto.Name}' hastanesi başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hastane oluşturma hatası");
            TempData["Error"] = ex.Message;
        }

        return View(dto);
    }

    // Hastane düzenleme formu
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var hospital = await _hospitalService.GetHospitalAsync(id, cancellationToken);
        if (hospital is null)
            return NotFound();

        var dto = new UpdateHospitalDto(
            hospital.Name, hospital.Street, hospital.District,
            hospital.City, hospital.PostalCode, hospital.Country,
            hospital.ContactEmail, hospital.Website);

        ViewBag.HospitalId = id;
        ViewBag.HospitalName = hospital.Name;
        return View(dto);
    }

    // Hastane bilgilerini kaydetme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UpdateHospitalDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.HospitalId = id;
            return View(dto);
        }

        try
        {
            await _hospitalService.UpdateHospitalAsync(id, dto, cancellationToken);
            TempData["Success"] = "Hastane bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hastane güncelleme hatası: {Id}", id);
            TempData["Error"] = ex.Message;
        }

        ViewBag.HospitalId = id;
        return View(dto);
    }

    // Hastaneyi pasife alma
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await _hospitalService.DeactivateHospitalAsync(id, cancellationToken);
            TempData["Success"] = "Hastane pasife alındı.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hastane pasife alma hatası: {Id}", id);
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
