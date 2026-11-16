using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Web.Controllers;

// Admin dashboard kontrolcüsü
[Authorize(Roles = "Admin,HospitalManager")]
public class AdminController : Controller
{
    private readonly IHospitalService _hospitalService;
    private readonly IAppointmentService _appointmentService;
    private readonly IDoctorService _doctorService;
    private readonly IPatientRepository _patientRepository;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IHospitalService hospitalService,
        IAppointmentService appointmentService,
        IDoctorService doctorService,
        IPatientRepository patientRepository,
        ILogger<AdminController> logger)
    {
        _hospitalService = hospitalService;
        _appointmentService = appointmentService;
        _doctorService = doctorService;
        _patientRepository = patientRepository;
        _logger = logger;
    }

    // Admin ana sayfası — gerçek PostgreSQL istatistikleri
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var hospitals = await _hospitalService.GetAllHospitalsAsync(cancellationToken);
        var appointments = await _appointmentService.GetAllAppointmentsAsync(null, cancellationToken);

        // Tüm şubeler ve bölümler toplamı
        var allBranches = hospitals.SelectMany(h => h.Branches ?? []).ToList();

        ViewBag.HospitalCount = hospitals.Count;
        ViewBag.BranchCount = allBranches.Count;
        ViewBag.DepartmentCount = allBranches.Sum(b => b.DepartmentCount);
        ViewBag.AppointmentCount = appointments.Count;
        ViewBag.PatientCount = await _patientRepository.CountAsync(cancellationToken: cancellationToken);

        // Doktor sayısı: ilk şubeden al
        int doctorCount = 0;
        foreach (var branch in allBranches.Take(10)) // performans için limit
        {
            var branchDoctors = await _doctorService.GetDoctorsByBranchAsync(branch.Id, cancellationToken);
            doctorCount += branchDoctors.Count;
        }
        ViewBag.DoctorCount = doctorCount;

        // Randevu durumları
        ViewBag.TodayAppointments = appointments
            .Count(a => a.AppointmentDate == DateOnly.FromDateTime(DateTime.Today));
        ViewBag.PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending);
        ViewBag.ConfirmedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Confirmed);
        ViewBag.CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled);
        ViewBag.CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed);

        return View();
    }
}
