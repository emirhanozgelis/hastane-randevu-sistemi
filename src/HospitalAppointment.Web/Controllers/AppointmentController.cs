using System.Security.Claims;
using HospitalAppointment.Application.DTOs.Appointment;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAppointment.Web.Controllers;

// Randevu yönetimi kontrolcüsü
[Authorize]
public class AppointmentController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IHospitalService _hospitalService;
    private readonly IBranchService _branchService;
    private readonly IDepartmentService _departmentService;
    private readonly IHealthcareServiceService _serviceService;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IHospitalService hospitalService,
        IBranchService branchService,
        IDepartmentService departmentService,
        IHealthcareServiceService serviceService,
        IPatientRepository patientRepository,
        ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _hospitalService = hospitalService;
        _branchService = branchService;
        _departmentService = departmentService;
        _serviceService = serviceService;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    // Randevu listesi — role göre filtreli
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        if (User.IsInRole("Admin") || User.IsInRole("HospitalManager"))
        {
            var allAppointments = await _appointmentService.GetAllAppointmentsAsync(null, cancellationToken);
            return View(allAppointments);
        }

        if (User.IsInRole("Doctor"))
        {
            var doctor = await _doctorService.GetDoctorByUserIdAsync(userId, cancellationToken);
            if (doctor is null)
            {
                TempData["Error"] = "Doktor kaydı bulunamadı.";
                return View(Array.Empty<AppointmentDto>());
            }
            var doctorAppointments = await _appointmentService.GetDoctorAppointmentsAsync(doctor.Id, null, cancellationToken);
            return View(doctorAppointments);
        }

        if (User.IsInRole("Patient"))
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId, cancellationToken);
            if (patient is null)
            {
                TempData["Error"] = "Hasta kaydı bulunamadı.";
                return View(Array.Empty<AppointmentDto>());
            }
            var patientAppointments = await _appointmentService.GetPatientAppointmentsAsync(patient.Id, cancellationToken);
            return View(patientAppointments);
        }

        return View(Array.Empty<AppointmentDto>());
    }

    // Randevu detayı
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetAppointmentAsync(id, cancellationToken);
        if (appointment is null)
            return NotFound();

        return View(appointment);
    }

    // Yeni randevu formu — sadece Hasta
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var vm = new AppointmentViewModel();
        await PopulateDropdowns(vm, cancellationToken);
        return View(vm);
    }

    // Randevu kaydetme — sadece Hasta
    [HttpPost]
    [Authorize(Roles = "Patient")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentViewModel vm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(vm, cancellationToken);
            return View(vm);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var patient = await _patientRepository.GetByUserIdAsync(userId, cancellationToken);
        if (patient is null)
        {
            TempData["Error"] = "Hasta kaydı bulunamadı.";
            await PopulateDropdowns(vm, cancellationToken);
            return View(vm);
        }

        try
        {
            var dto = new CreateAppointmentDto(
                vm.HospitalId,
                vm.BranchId,
                vm.DepartmentId,
                vm.DoctorId,
                vm.HealthcareServiceId,
                vm.AppointmentDate,
                vm.StartTime,
                vm.Notes);

            await _appointmentService.CreateAppointmentAsync(patient.Id, dto, cancellationToken);
            TempData["Success"] = "Randevunuz başarıyla oluşturuldu.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Randevu oluşturma hatası");
            TempData["Error"] = ex.Message;
        }

        await PopulateDropdowns(vm, cancellationToken);
        return View(vm);
    }

    // ─── Durum Geçişleri ──────────────────────────────────────────────────────

    // Randevuyu onayla — Doctor/Admin
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager,Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        try
        {
            await _appointmentService.ConfirmAppointmentAsync(id, userId, null, cancellationToken);
            TempData["Success"] = "Randevu onaylandı.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Randevuyu reddet — Doctor/Admin
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager,Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(Guid id, string reason, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        try
        {
            await _appointmentService.RejectAppointmentAsync(id, userId, reason, cancellationToken);
            TempData["Success"] = "Randevu reddedildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Randevuyu iptal et — Doctor/Admin/Patient
    [HttpPost]
    [Authorize(Roles = "Admin,HospitalManager,Doctor,Patient")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id, string? reason, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        try
        {
            await _appointmentService.CancelAppointmentAsync(id, userId, reason, cancellationToken);
            TempData["Success"] = "Randevu iptal edildi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // Randevuyu tamamla — Doctor/Admin
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, string? notes, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";
        try
        {
            await _appointmentService.CompleteAppointmentAsync(id, userId, notes, cancellationToken);
            TempData["Success"] = "Randevu tamamlandı.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    // ─── Cascading Dropdown AJAX Endpoint'leri ────────────────────────────────

    // Hastaneye göre şubeleri döndürür — JSON
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetBranches(Guid hospitalId, CancellationToken cancellationToken)
    {
        if (hospitalId == Guid.Empty) return Json(Array.Empty<object>());
        try
        {
            var branches = await _branchService.GetBranchesByHospitalAsync(hospitalId, cancellationToken);
            return Json(branches.Select(b => new { value = b.Id, text = b.Name }));
        }
        catch { return Json(Array.Empty<object>()); }
    }

    // Şubeye göre bölümleri döndürür — JSON
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDepartments(Guid branchId, CancellationToken cancellationToken)
    {
        if (branchId == Guid.Empty) return Json(Array.Empty<object>());
        try
        {
            var departments = await _departmentService.GetDepartmentsByBranchAsync(branchId, cancellationToken);
            return Json(departments.Select(d => new { value = d.Id, text = d.Name }));
        }
        catch { return Json(Array.Empty<object>()); }
    }

    // Bölüme göre doktorları döndürür — JSON
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetDoctors(Guid departmentId, CancellationToken cancellationToken)
    {
        if (departmentId == Guid.Empty) return Json(Array.Empty<object>());
        try
        {
            var doctors = await _doctorService.GetDoctorsByDepartmentAsync(departmentId, cancellationToken);
            return Json(doctors.Select(d => new { value = d.Id, text = d.FullName + " — " + d.Specialization }));
        }
        catch { return Json(Array.Empty<object>()); }
    }

    // Bölüme göre sağlık hizmetlerini döndürür — JSON (doktor seçiminden bağımsız)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetServices(Guid departmentId, CancellationToken cancellationToken)
    {
        if (departmentId == Guid.Empty) return Json(Array.Empty<object>());
        try
        {
            var services = await _serviceService.GetServicesByDepartmentAsync(departmentId, cancellationToken);
            return Json(services.Select(s => new
            {
                value = s.Id,
                text = $"{s.Name} ({s.DurationMinutes} dk — {s.Price:N2} TRY)"
            }));
        }
        catch { return Json(Array.Empty<object>()); }
    }

    // ─── Yardımcı Metot ───────────────────────────────────────────────────────

    private async Task PopulateDropdowns(AppointmentViewModel vm, CancellationToken cancellationToken)
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);
        vm.Hospitals = new SelectList(hospitals, "Id", "Name", vm.HospitalId);

        if (vm.HospitalId != Guid.Empty)
        {
            var branches = await _branchService.GetBranchesByHospitalAsync(vm.HospitalId, cancellationToken);
            vm.Branches = new SelectList(branches, "Id", "Name", vm.BranchId);
        }
        else
        {
            vm.Branches = new SelectList(Enumerable.Empty<object>(), "Id", "Name");
        }

        if (vm.BranchId != Guid.Empty)
        {
            var departments = await _departmentService.GetDepartmentsByBranchAsync(vm.BranchId, cancellationToken);
            vm.Departments = new SelectList(departments, "Id", "Name", vm.DepartmentId);
        }
        else
        {
            vm.Departments = new SelectList(Enumerable.Empty<object>(), "Id", "Name");
        }

        if (vm.DepartmentId != Guid.Empty)
        {
            var doctors = await _doctorService.GetDoctorsByDepartmentAsync(vm.DepartmentId, cancellationToken);
            vm.Doctors = new SelectList(doctors, "Id", "FullName", vm.DoctorId);

            var services = await _serviceService.GetServicesByDepartmentAsync(vm.DepartmentId, cancellationToken);
            vm.Services = new SelectList(services, "Id", "Name", vm.HealthcareServiceId);
        }
        else
        {
            vm.Doctors = new SelectList(Enumerable.Empty<object>(), "Id", "FullName");
            vm.Services = new SelectList(Enumerable.Empty<object>(), "Id", "Name");
        }
    }
}
