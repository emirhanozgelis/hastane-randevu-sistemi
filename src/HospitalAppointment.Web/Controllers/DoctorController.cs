using System.Security.Claims;
using HospitalAppointment.Application.DTOs.Doctor;
using HospitalAppointment.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAppointment.Web.Controllers;

// Doktor yönetimi kontrolcüsü
public class DoctorController : Controller
{
    private readonly IDoctorService _doctorService;
    private readonly IBranchService _branchService;
    private readonly IDepartmentService _departmentService;
    private readonly ILogger<DoctorController> _logger;

    public DoctorController(
        IDoctorService doctorService,
        IBranchService branchService,
        IDepartmentService departmentService,
        ILogger<DoctorController> logger)
    {
        _doctorService = doctorService;
        _branchService = branchService;
        _departmentService = departmentService;
        _logger = logger;
    }

    // Doktor listesi — herkese açık
    public async Task<IActionResult> Index(
        Guid? departmentId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<DoctorDto> doctors;

        if (departmentId.HasValue && departmentId != Guid.Empty)
        {
            doctors = await _doctorService.GetDoctorsByDepartmentAsync(departmentId.Value, cancellationToken);
            var dept = await _departmentService.GetDepartmentAsync(departmentId.Value, cancellationToken);
            ViewBag.FilterName = dept?.Name;
        }
        else if (branchId.HasValue && branchId != Guid.Empty)
        {
            doctors = await _doctorService.GetDoctorsByBranchAsync(branchId.Value, cancellationToken);
            var branch = await _branchService.GetBranchAsync(branchId.Value, cancellationToken);
            ViewBag.FilterName = branch?.Name;
        }
        else
        {
            doctors = new List<DoctorDto>();
        }

        return View(doctors);
    }

    // Doktor detay sayfası — herkese açık
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorAsync(id, cancellationToken);
        if (doctor is null)
            return NotFound();

        var availability = await _doctorService.GetDoctorAvailabilityAsync(id, cancellationToken);
        ViewBag.Availability = availability;
        return View(doctor);
    }

    // Yeni doktor oluşturma formu — Admin veya HospitalManager
    [HttpGet]
    [Authorize(Roles = "Admin,HospitalManager")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateDropdowns(null, null, cancellationToken);
        return View(new CreateDoctorDto(
            string.Empty, string.Empty, string.Empty, string.Empty,
            DateOnly.FromDateTime(DateTime.Today.AddYears(-30)),
            HospitalAppointment.Domain.Enums.Gender.NotSpecified,
            string.Empty, DateOnly.FromDateTime(DateTime.Today),
            Guid.Empty, Guid.Empty,
            string.Empty, string.Empty, string.Empty));
    }

    // Yeni doktor kaydetme — Admin veya HospitalManager
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDoctorDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(dto.BranchId, dto.DepartmentId, cancellationToken);
            return View(dto);
        }

        try
        {
            await _doctorService.CreateDoctorAsync(dto, cancellationToken);
            TempData["Success"] = $"Dr. {dto.FirstName} {dto.LastName} başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index), new { branchId = dto.BranchId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doktor oluşturma hatası");
            TempData["Error"] = ex.Message;
        }

        await PopulateDropdowns(dto.BranchId, dto.DepartmentId, cancellationToken);
        return View(dto);
    }

    // Doktor müsaitlik takvimi — herkese açık
    [HttpGet]
    public async Task<IActionResult> Availability(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorAsync(id, cancellationToken);
        if (doctor is null)
            return NotFound();

        var availability = await _doctorService.GetDoctorAvailabilityAsync(id, cancellationToken);
        ViewBag.Doctor = doctor;
        return View(availability);
    }

    // Müsaitlik ekleme formu — Admin veya Doctor
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor")]
    public async Task<IActionResult> AddAvailability(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorAsync(id, cancellationToken);
        if (doctor is null)
            return NotFound();

        ViewBag.DoctorId = id;
        ViewBag.DoctorName = doctor.FullName;
        return View(new CreateDoctorAvailabilityDto(id, null, null,
            new TimeOnly(9, 0), new TimeOnly(17, 0)));
    }

    // Müsaitlik kaydetme — Admin veya Doctor
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAvailability(
        Guid id,
        CreateDoctorAvailabilityDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.DoctorId = id;
            return View(dto);
        }

        try
        {
            await _doctorService.AddAvailabilityAsync(dto, cancellationToken);
            TempData["Success"] = "Müsaitlik kaydedildi.";
            return RedirectToAction(nameof(Availability), new { id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Müsaitlik ekleme hatası: DoctorId={Id}", id);
            TempData["Error"] = ex.Message;
        }

        ViewBag.DoctorId = id;
        return View(dto);
    }

    private async Task PopulateDropdowns(
        Guid? branchId,
        Guid? departmentId,
        CancellationToken cancellationToken)
    {
        // Şubeler — tüm hastanelerin şubelerini göstermek için kısaltılmış örnek
        // Gerçek uygulamada IHospitalService.GetAllHospitalsAsync üzerinden tüm şubeler yüklenir
        IReadOnlyList<HospitalAppointment.Application.DTOs.Branch.BranchDto> branches =
            branchId.HasValue ? Array.Empty<HospitalAppointment.Application.DTOs.Branch.BranchDto>()
                             : Array.Empty<HospitalAppointment.Application.DTOs.Branch.BranchDto>();

        IReadOnlyList<HospitalAppointment.Application.DTOs.Department.DepartmentDto> departments =
            branchId.HasValue && branchId != Guid.Empty
                ? await _departmentService.GetDepartmentsByBranchAsync(branchId.Value, cancellationToken)
                : Array.Empty<HospitalAppointment.Application.DTOs.Department.DepartmentDto>();

        ViewBag.Branches = new SelectList(branches, "Id", "Name", branchId);
        ViewBag.Departments = new SelectList(departments, "Id", "Name", departmentId);
    }
}
