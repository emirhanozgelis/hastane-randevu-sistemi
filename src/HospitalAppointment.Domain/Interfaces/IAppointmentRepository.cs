using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

// Randevu repository arayüzü — ISP gereği özelleştirilmiş interface
public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IReadOnlyList<Appointment>> GetByDoctorIdAsync(Guid doctorId, DateOnly? date = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<bool> HasDoctorConflictAsync(Guid doctorId, DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<bool> HasPatientConflictAsync(Guid patientId, DateOnly date, TimeOnly startTime, TimeOnly endTime, Guid? excludeId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetWithDetailsAsync(Guid? patientId = null, Guid? doctorId = null, Guid? branchId = null, CancellationToken cancellationToken = default);
    Task<Appointment?> GetWithDetailsAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    // Durum geçişi için doğrudan güncelleme — EF change tracking sorunlarını atlatır
    Task UpdateStatusAsync(Guid appointmentId, Domain.Enums.AppointmentStatus status, string? doctorNotes, string? rejectionReason, CancellationToken cancellationToken = default);
    Task AddStatusHistoryAsync(Guid appointmentId, Domain.Enums.AppointmentStatus status, string changedBy, string? reason, CancellationToken cancellationToken = default);
}
