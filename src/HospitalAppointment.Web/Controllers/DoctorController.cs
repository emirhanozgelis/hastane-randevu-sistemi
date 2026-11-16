using HospitalAppointment.Application.DTOs.Doctor;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Web.Models;
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
    private readonly IHospitalService _hospitalService;
    private readonly ILogger<DoctorController> _logger;

    public DoctorController(
        IDoctorService doctorService,
        IBranchService branchService,
        IDepartmentService departmentService,
        IHospitalService hospitalService,
        ILogger<DoctorController> logger)
    {
        _doctorService = doctorService;
        _branchService = branchService;
        _departmentService = departmentService;
        _hospitalService = hospitalService;
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
            // Filtre yok — tüm şubelerdeki doktorları getir
            var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);
            var allDoctors = new List<DoctorDto>();
            foreach (var h in hospitals)
                foreach (var b in h.Branches ?? [])
                    allDoctors.AddRange(await _doctorService.GetDoctorsByBranchAsync(b.Id, cancellationToken));
            doctors = allDoctors.DistinctBy(d => d.Id).ToList().AsReadOnly();
        }

        return View(doctors);
    }

    // Doktor detay sayfası — herkese açık
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorAsync(id, cancellationToken);
        if (doctor is null) return NotFound();

        var availability = await _doctorService.GetDoctorAvailabilityAsync(id, cancellationToken);
        ViewBag.Availability = availability;
        return View(doctor);
    }

    // Yeni doktor oluşturma formu — Admin veya HospitalManager
    [HttpGet]
    [Authorize(Roles = "Admin,HospitalManager")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var vm = new CreateDoctorViewModel();
        await PopulateDropdowns(vm, cancellationToken);
        return View(vm);
    }

    // Yeni doktor kaydetme — ViewModel üzerinden, DTO'ya dönüştürülür
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateDoctorViewModel vm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(vm, cancellationToken);
            return View(vm);
        }

        try
        {
            // ViewModel → DTO dönüşümü
            var dto = new CreateDoctorDto(
                vm.FirstName, vm.LastName, vm.Email, vm.PhoneNumber,
                vm.DateOfBirth, vm.Gender,
                vm.EmployeeNumber, vm.HireDate,
                vm.BranchId, vm.DepartmentId,
                vm.Specialization, vm.LicenseNumber,
                vm.Password, vm.Biography);

            await _doctorService.CreateDoctorAsync(dto, cancellationToken);
            TempData["Success"] = $"Dr. {vm.FirstName} {vm.LastName} başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index), new { branchId = vm.BranchId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Doktor oluşturma hatası");
            TempData["Error"] = ex.Message;
        }

        await PopulateDropdowns(vm, cancellationToken);
        return View(vm);
    }

    // Doktor müsaitlik takvimi — herkese açık
    [HttpGet]
    public async Task<IActionResult> Availability(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetDoctorAsync(id, cancellationToken);
        if (doctor is null) return NotFound();

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
        if (doctor is null) return NotFound();

        ViewBag.DoctorId = id;
        ViewBag.DoctorName = doctor.FullName;
        return View(new CreateDoctorAvailabilityDto(id, null, null,
            new TimeOnly(9, 0), new TimeOnly(17, 0)));
    }

    // Müsaitlik kaydetme
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

    // ViewModel'e şube ve bölüm dropdown'larını doldur
    private async Task PopulateDropdowns(CreateDoctorViewModel vm, CancellationToken cancellationToken)
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);
        var allBranches = hospitals.SelectMany(h => h.Branches ?? []).ToList();

        vm.Branches = new SelectList(allBranches, "Id", "Name", vm.BranchId);

        if (vm.BranchId != Guid.Empty)
        {
            var departments = await _departmentService.GetDepartmentsByBranchAsync(vm.BranchId, cancellationToken);
            vm.Departments = new SelectList(departments, "Id", "Name", vm.DepartmentId);
        }
        else
        {
            vm.Departments = new SelectList(Enumerable.Empty<object>(), "Id", "Name");
        }
    }
}
