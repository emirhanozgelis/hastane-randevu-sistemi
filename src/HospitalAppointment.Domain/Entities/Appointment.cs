using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Exceptions;
using HospitalAppointment.Domain.ValueObjects;

namespace HospitalAppointment.Domain.Entities;

// Randevu entity'si — iş kuralları entity içinde kapsüllenmiş
public class Appointment : BaseEntity
{
    public Guid HospitalId { get; private set; }
    public Guid BranchId { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid HealthcareServiceId { get; private set; }

    public DateOnly AppointmentDate { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    // Randevu oluşturulurken anlık fiyat ve süre snapshot olarak kaydedilir
    public Money SnapshotPrice { get; private set; }
    public TimeSpan SnapshotDuration { get; private set; }
    public string SnapshotServiceName { get; private set; }

    public AppointmentStatus Status { get; private set; }
    public string? PatientNotes { get; private set; }
    public string? DoctorNotes { get; private set; }
    public string? RejectionReason { get; private set; }

    // Navigasyon özellikleri
    public Hospital? Hospital { get; private set; }
    public Branch? Branch { get; private set; }
    public Department? Department { get; private set; }
    public Doctor? Doctor { get; private set; }
    public Patient? Patient { get; private set; }
    public HealthcareService? HealthcareService { get; private set; }

    private readonly List<AppointmentStatusHistory> _statusHistory = [];
    public IReadOnlyCollection<AppointmentStatusHistory> StatusHistory => _statusHistory.AsReadOnly();

    private Appointment()
    {
        SnapshotPrice = Money.Zero();
        SnapshotServiceName = "";
    }

    public Appointment(
        Guid hospitalId,
        Guid branchId,
        Guid departmentId,
        Guid doctorId,
        Guid patientId,
        Guid healthcareServiceId,
        DateOnly appointmentDate,
        TimeOnly startTime,
        TimeOnly endTime,
        Money snapshotPrice,
        TimeSpan snapshotDuration,
        string snapshotServiceName,
        string? patientNotes = null)
    {
        ValidateGuids(hospitalId, branchId, departmentId, doctorId, patientId, healthcareServiceId);
        ValidateDateAndTime(appointmentDate, startTime, endTime);
        ArgumentNullException.ThrowIfNull(snapshotPrice);

        HospitalId = hospitalId;
        BranchId = branchId;
        DepartmentId = departmentId;
        DoctorId = doctorId;
        PatientId = patientId;
        HealthcareServiceId = healthcareServiceId;
        AppointmentDate = appointmentDate;
        StartTime = startTime;
        EndTime = endTime;
        SnapshotPrice = snapshotPrice;
        SnapshotDuration = snapshotDuration;
        SnapshotServiceName = snapshotServiceName?.Trim() ?? string.Empty;
        PatientNotes = patientNotes?.Trim();
        Status = AppointmentStatus.Pending;

        // İlk durum geçmişi kaydı
        AddStatusHistory(AppointmentStatus.Pending, "system", "Randevu oluşturuldu.");
    }

    // Durum geçiş iş kuralları — kapsülleme
    public void Confirm(string confirmedBy, string? notes = null)
    {
        EnsureStatusTransitionAllowed(AppointmentStatus.Confirmed);
        Status = AppointmentStatus.Confirmed;
        DoctorNotes = notes?.Trim();
        AddStatusHistory(AppointmentStatus.Confirmed, confirmedBy, notes);
        MarkUpdated();
    }

    public void Reject(string rejectedBy, string reason)
    {
        EnsureStatusTransitionAllowed(AppointmentStatus.Rejected);
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Red nedeni belirtilmelidir.", "REJECTION_REASON_REQUIRED");

        Status = AppointmentStatus.Rejected;
        RejectionReason = reason.Trim();
        AddStatusHistory(AppointmentStatus.Rejected, rejectedBy, reason);
        MarkUpdated();
    }

    public void Cancel(string cancelledBy, string? reason = null)
    {
        EnsureStatusTransitionAllowed(AppointmentStatus.Cancelled);
        Status = AppointmentStatus.Cancelled;
        AddStatusHistory(AppointmentStatus.Cancelled, cancelledBy, reason);
        MarkUpdated();
    }

    public void Complete(string completedBy, string? doctorNotes = null)
    {
        EnsureStatusTransitionAllowed(AppointmentStatus.Completed);
        Status = AppointmentStatus.Completed;
        DoctorNotes = doctorNotes?.Trim();
        AddStatusHistory(AppointmentStatus.Completed, completedBy, doctorNotes);
        MarkUpdated();
    }

    public void MarkAsNoShow(string markedBy)
    {
        EnsureStatusTransitionAllowed(AppointmentStatus.NoShow);
        Status = AppointmentStatus.NoShow;
        AddStatusHistory(AppointmentStatus.NoShow, markedBy, "Hasta randevuya gelmedi.");
        MarkUpdated();
    }

    // Çakışma kontrolü: NewStart < ExistingEnd AND NewEnd > ExistingStart
    public bool ConflictsWith(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        if (AppointmentDate != date) return false;
        if (Status is AppointmentStatus.Cancelled or AppointmentStatus.Rejected) return false;
        return startTime < EndTime && endTime > StartTime;
    }

    private void EnsureStatusTransitionAllowed(AppointmentStatus targetStatus)
    {
        var allowedTransitions = new Dictionary<AppointmentStatus, HashSet<AppointmentStatus>>
        {
            [AppointmentStatus.Pending] = [AppointmentStatus.Confirmed, AppointmentStatus.Rejected, AppointmentStatus.Cancelled],
            [AppointmentStatus.Confirmed] = [AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow],
            [AppointmentStatus.Rejected] = [],
            [AppointmentStatus.Cancelled] = [],
            [AppointmentStatus.Completed] = [],
            [AppointmentStatus.NoShow] = []
        };

        if (!allowedTransitions[Status].Contains(targetStatus))
            throw new InvalidStatusTransitionException(Status, targetStatus);
    }

    private void AddStatusHistory(AppointmentStatus status, string changedBy, string? reason)
    {
        _statusHistory.Add(new AppointmentStatusHistory(Id, status, changedBy, reason));
    }

    private static void ValidateGuids(params Guid[] ids)
    {
        foreach (var id in ids)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Kimlik değeri geçersiz (boş GUID).");
        }
    }

    private static void ValidateDateAndTime(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        if (date < DateOnly.FromDateTime(DateTime.Today))
            throw new DomainException("Geçmiş bir tarih için randevu oluşturulamaz.", "PAST_DATE");
        if (startTime >= endTime)
            throw new DomainException("Başlangıç saati bitiş saatinden önce olmalıdır.", "INVALID_TIME_RANGE");
    }
}
