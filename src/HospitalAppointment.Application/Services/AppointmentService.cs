using HospitalAppointment.Application.DTOs.Appointment;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Exceptions;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Application.Services;

public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IPatientRepository _patientRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IHealthcareServiceRepository _healthcareServiceRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IPatientRepository patientRepository,
        IDoctorRepository doctorRepository,
        IHealthcareServiceRepository healthcareServiceRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _patientRepository = patientRepository;
        _doctorRepository = doctorRepository;
        _healthcareServiceRepository = healthcareServiceRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AppointmentDto> CreateAppointmentAsync(
        Guid patientId,
        CreateAppointmentDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu oluşturuluyor — Hasta: {PatientId}, Doktor: {DoctorId}", patientId, dto.DoctorId);

        var patient = await _patientRepository.GetByIdAsync(patientId, cancellationToken)
            ?? throw new KeyNotFoundException($"Hasta bulunamadı: {patientId}");

        var doctor = await _doctorRepository.GetWithDetailsAsync(dto.DoctorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Doktor bulunamadı: {dto.DoctorId}");

        var service = await _healthcareServiceRepository.GetByIdAsync(dto.HealthcareServiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sağlık hizmeti bulunamadı: {dto.HealthcareServiceId}");

        if (!service.IsActive)
            throw new DomainException("Seçilen sağlık hizmeti aktif değil.", "SERVICE_INACTIVE");

        var endTime = dto.StartTime.Add(service.Duration);

        // Concurrency güvenliği: PostgreSQL transaction-scoped advisory lock.
        // Hem doktor hem hasta ID'si kilitlenir → aynı doktor VEYA aynı hasta çakışması önlenir.
        // Tutarlı lock sırası (küçük key önce) → deadlock riski yok.
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // Lock → müsaitlik → çakışma kontrolü → kayıt: atomik işlem
            await _unitOfWork.AcquireAppointmentLocksAsync(dto.DoctorId, patientId, cancellationToken);

            // Doktor uygunluk kontrolü — ödev gereksinimi: "hastalar sadece bu zaman aralıklarına randevu alabilecek"
            if (!doctor.IsAvailableAt(dto.AppointmentDate, dto.StartTime, endTime))
                throw new DoctorNotAvailableException(doctor.FullName, dto.AppointmentDate, dto.StartTime);

            var hasDoctorConflict = await _appointmentRepository.HasDoctorConflictAsync(
                dto.DoctorId, dto.AppointmentDate, dto.StartTime, endTime,
                cancellationToken: cancellationToken);

            if (hasDoctorConflict)
                throw new AppointmentConflictException("Seçilen saat aralığında doktora ait başka bir randevu mevcut.");

            var hasPatientConflict = await _appointmentRepository.HasPatientConflictAsync(
                patientId, dto.AppointmentDate, dto.StartTime, endTime,
                cancellationToken: cancellationToken);

            if (hasPatientConflict)
                throw new AppointmentConflictException("Seçilen saat aralığında hastaya ait başka bir randevu mevcut.");

            var appointment = new Appointment(
                hospitalId: dto.HospitalId,
                branchId: dto.BranchId,
                departmentId: dto.DepartmentId,
                doctorId: dto.DoctorId,
                patientId: patientId,
                healthcareServiceId: dto.HealthcareServiceId,
                appointmentDate: dto.AppointmentDate,
                startTime: dto.StartTime,
                endTime: endTime,
                snapshotPrice: service.Price,
                snapshotDuration: service.Duration,
                snapshotServiceName: service.Name,
                patientNotes: dto.Notes);

            await _appointmentRepository.AddAsync(appointment, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Randevu oluşturuldu — Id: {AppointmentId}", appointment.Id);

            await _notificationService.SendAppointmentCreatedAsync(
                patient.UserId, appointment.Id, service.Name,
                dto.AppointmentDate, dto.StartTime, cancellationToken);

            var created = await _appointmentRepository.GetWithDetailsAsync(appointment.Id, cancellationToken);
            return created is not null ? MapToDto(created) : MapToBasicDto(appointment, patient, doctor, service);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<AppointmentDto?> GetAppointmentAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var appointment = await _appointmentRepository.GetWithDetailsAsync(appointmentId, cancellationToken);
        return appointment is null ? null : MapToDto(appointment);
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetPatientAppointmentsAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetWithDetailsAsync(
            patientId: patientId,
            cancellationToken: cancellationToken);

        return appointments.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetDoctorAppointmentsAsync(
        Guid doctorId,
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId, date, cancellationToken);

        // Navigasyon özellikleri yüklü değilse detaylı sorgula
        var detailedAppointments = await _appointmentRepository.GetWithDetailsAsync(
            doctorId: doctorId,
            cancellationToken: cancellationToken);

        var result = detailedAppointments;
        if (date.HasValue)
            result = detailedAppointments.Where(a => a.AppointmentDate == date.Value).ToList().AsReadOnly();

        return result.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<AppointmentDto>> GetAllAppointmentsAsync(
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetWithDetailsAsync(
            branchId: branchId,
            cancellationToken: cancellationToken);

        return appointments.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task ConfirmAppointmentAsync(
        Guid appointmentId,
        string confirmedBy,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu onaylanıyor — Id: {AppointmentId}", appointmentId);

        var appointment = await _appointmentRepository.GetWithDetailsAsync(appointmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Randevu bulunamadı: {appointmentId}");

        // Domain kuralı kontrolü — geçersiz geçişte exception fırlatır
        appointment.Confirm(confirmedBy, notes);

        // Raw SQL ile güncelle — EF change tracking sorunu bypass edilir
        await _appointmentRepository.UpdateStatusAsync(appointmentId, AppointmentStatus.Confirmed, notes, null, cancellationToken);
        await _appointmentRepository.AddStatusHistoryAsync(appointmentId, AppointmentStatus.Confirmed, confirmedBy, notes, cancellationToken);
        // Raw SQL ile güncelleme yapıldı — EF tracker'ı temizle (xmin uyumsuzluğunu önle)
        _unitOfWork.ClearTracker();

        if (appointment.Patient is not null)
        {
            await _notificationService.SendAppointmentConfirmedAsync(
                appointment.Patient.UserId, appointment.Id,
                appointment.Doctor?.FullName ?? "Doktor",
                appointment.AppointmentDate, appointment.StartTime, cancellationToken);
        }
    }

    public async Task RejectAppointmentAsync(
        Guid appointmentId,
        string rejectedBy,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu reddediliyor — Id: {AppointmentId}", appointmentId);

        var appointment = await _appointmentRepository.GetWithDetailsAsync(appointmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Randevu bulunamadı: {appointmentId}");

        appointment.Reject(rejectedBy, reason);

        await _appointmentRepository.UpdateStatusAsync(appointmentId, AppointmentStatus.Rejected, null, reason, cancellationToken);
        await _appointmentRepository.AddStatusHistoryAsync(appointmentId, AppointmentStatus.Rejected, rejectedBy, reason, cancellationToken);
        _unitOfWork.ClearTracker();

        if (appointment.Patient is not null)
        {
            await _notificationService.SendAppointmentRejectedAsync(
                appointment.Patient.UserId, appointment.Id, reason, cancellationToken);
        }
    }

    public async Task CancelAppointmentAsync(
        Guid appointmentId,
        string cancelledBy,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu iptal ediliyor — Id: {AppointmentId}", appointmentId);

        var appointment = await _appointmentRepository.GetWithDetailsAsync(appointmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Randevu bulunamadı: {appointmentId}");

        appointment.Cancel(cancelledBy, reason);

        await _appointmentRepository.UpdateStatusAsync(appointmentId, AppointmentStatus.Cancelled, null, null, cancellationToken);
        await _appointmentRepository.AddStatusHistoryAsync(appointmentId, AppointmentStatus.Cancelled, cancelledBy, reason, cancellationToken);
        _unitOfWork.ClearTracker();

        if (appointment.Patient is not null)
        {
            await _notificationService.SendAppointmentCancelledAsync(
                appointment.Patient.UserId, appointment.Id, cancellationToken);
        }
    }

    public async Task CompleteAppointmentAsync(
        Guid appointmentId,
        string completedBy,
        string? doctorNotes = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu tamamlanıyor — Id: {AppointmentId}", appointmentId);

        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Randevu bulunamadı: {appointmentId}");

        appointment.Complete(completedBy, doctorNotes);

        await _appointmentRepository.UpdateStatusAsync(appointmentId, AppointmentStatus.Completed, doctorNotes, null, cancellationToken);
        await _appointmentRepository.AddStatusHistoryAsync(appointmentId, AppointmentStatus.Completed, completedBy, doctorNotes, cancellationToken);
    }

    public async Task MarkAsNoShowAsync(
        Guid appointmentId,
        string markedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu 'gelmedi' olarak işaretleniyor — Id: {AppointmentId}", appointmentId);

        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Randevu bulunamadı: {appointmentId}");

        appointment.MarkAsNoShow(markedBy);

        await _appointmentRepository.UpdateStatusAsync(appointmentId, AppointmentStatus.NoShow, null, null, cancellationToken);
        await _appointmentRepository.AddStatusHistoryAsync(appointmentId, AppointmentStatus.NoShow, markedBy, "Hasta randevuya gelmedi.", cancellationToken);
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        Guid doctorId,
        Guid serviceId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        // Doktoru müsaitlik kayıtlarıyla yükle
        var doctor = await _doctorRepository.GetWithDetailsAsync(doctorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Doktor bulunamadı: {doctorId}");

        var service = await _healthcareServiceRepository.GetByIdAsync(serviceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sağlık hizmeti bulunamadı: {serviceId}");

        // O gün için müsaitlik penceresi — domain metodu
        var dayOfWeek = date.DayOfWeek;
        var availability = doctor.Availabilities
            .Where(a => a.IsAvailable)
            .FirstOrDefault(a =>
                (a.IsSpecificDate && a.Date == date) ||
                (!a.IsSpecificDate && a.DayOfWeek == dayOfWeek));

        if (availability == null)
            return [];   // O gün müsait değil

        // Mevcut randevular — iptal/red hariç
        var existingAppointments = await _appointmentRepository.GetByDoctorIdAsync(doctorId, date, cancellationToken);
        var bookedSlots = existingAppointments
            .Where(a => a.Status is not AppointmentStatus.Cancelled and not AppointmentStatus.Rejected)
            .Select(a => (a.StartTime, a.EndTime))
            .ToList();

        // Hizmet süresine göre slotlar üret
        var slots = new List<AvailableSlotDto>();
        var slotStart = availability.StartTime;
        var duration = service.Duration;

        while (slotStart.Add(duration) <= availability.EndTime)
        {
            var slotEnd = slotStart.Add(duration);

            // Mevcut randevuyla çakışıyor mu — NewStart < ExistingEnd AND NewEnd > ExistingStart
            var hasConflict = bookedSlots.Any(b =>
                slotStart < b.EndTime && slotEnd > b.StartTime);

            if (!hasConflict)
            {
                slots.Add(new AvailableSlotDto(
                    StartTime: slotStart,
                    EndTime: slotEnd,
                    Display: $"{slotStart:HH:mm} – {slotEnd:HH:mm}",
                    StartTimeValue: slotStart.ToString("HH:mm:ss")));
            }

            slotStart = slotStart.Add(duration);
        }

        return slots.AsReadOnly();
    }

    // Navigasyon özellikleri yüklü Appointment -> AppointmentDto dönüşümü
    private static AppointmentDto MapToDto(Appointment a) => new(
        Id: a.Id,
        HospitalId: a.HospitalId,
        HospitalName: a.Hospital?.Name ?? string.Empty,
        BranchId: a.BranchId,
        BranchName: a.Branch?.Name ?? string.Empty,
        DepartmentId: a.DepartmentId,
        DepartmentName: a.Department?.Name ?? string.Empty,
        DoctorId: a.DoctorId,
        DoctorName: a.Doctor?.FullName ?? string.Empty,
        DoctorSpecialization: a.Doctor?.Specialization ?? string.Empty,
        PatientId: a.PatientId,
        PatientName: a.Patient?.FullName ?? string.Empty,
        HealthcareServiceId: a.HealthcareServiceId,
        ServiceName: a.SnapshotServiceName,
        AppointmentDate: a.AppointmentDate,
        StartTime: a.StartTime,
        EndTime: a.EndTime,
        Price: a.SnapshotPrice.Amount,
        Currency: a.SnapshotPrice.Currency,
        Status: a.Status,
        StatusDisplay: GetStatusDisplay(a.Status),
        PatientNotes: a.PatientNotes,
        DoctorNotes: a.DoctorNotes,
        RejectionReason: a.RejectionReason,
        CreatedAt: a.CreatedAt);

    // Navigasyon özellikleri yüklenemediğinde temel entity'lerden oluşturulur
    private static AppointmentDto MapToBasicDto(
        Appointment a, Patient patient, Doctor doctor, HealthcareService service) => new(
        Id: a.Id,
        HospitalId: a.HospitalId,
        HospitalName: string.Empty,
        BranchId: a.BranchId,
        BranchName: string.Empty,
        DepartmentId: a.DepartmentId,
        DepartmentName: string.Empty,
        DoctorId: a.DoctorId,
        DoctorName: doctor.FullName,
        DoctorSpecialization: doctor.Specialization,
        PatientId: a.PatientId,
        PatientName: patient.FullName,
        HealthcareServiceId: a.HealthcareServiceId,
        ServiceName: a.SnapshotServiceName,
        AppointmentDate: a.AppointmentDate,
        StartTime: a.StartTime,
        EndTime: a.EndTime,
        Price: a.SnapshotPrice.Amount,
        Currency: a.SnapshotPrice.Currency,
        Status: a.Status,
        StatusDisplay: GetStatusDisplay(a.Status),
        PatientNotes: a.PatientNotes,
        DoctorNotes: a.DoctorNotes,
        RejectionReason: a.RejectionReason,
        CreatedAt: a.CreatedAt);

    private static string GetStatusDisplay(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Pending => "Beklemede",
        AppointmentStatus.Confirmed => "Onaylandı",
        AppointmentStatus.Rejected => "Reddedildi",
        AppointmentStatus.Cancelled => "İptal Edildi",
        AppointmentStatus.Completed => "Tamamlandı",
        AppointmentStatus.NoShow => "Gelmedi",
        _ => status.ToString()
    };
}
