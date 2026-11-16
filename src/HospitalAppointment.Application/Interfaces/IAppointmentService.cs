using HospitalAppointment.Application.DTOs.Appointment;
using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Application.Interfaces;

// ISP gereği randevuya özgü servis arayüzü
public interface IAppointmentService
{
    Task<AppointmentDto> CreateAppointmentAsync(Guid patientId, CreateAppointmentDto dto, CancellationToken cancellationToken = default);
    Task<AppointmentDto?> GetAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppointmentDto>> GetPatientAppointmentsAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppointmentDto>> GetDoctorAppointmentsAsync(Guid doctorId, DateOnly? date = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppointmentDto>> GetAllAppointmentsAsync(Guid? branchId = null, CancellationToken cancellationToken = default);
    Task ConfirmAppointmentAsync(Guid appointmentId, string confirmedBy, string? notes = null, CancellationToken cancellationToken = default);
    Task RejectAppointmentAsync(Guid appointmentId, string rejectedBy, string reason, CancellationToken cancellationToken = default);
    Task CancelAppointmentAsync(Guid appointmentId, string cancelledBy, string? reason = null, CancellationToken cancellationToken = default);
    Task CompleteAppointmentAsync(Guid appointmentId, string completedBy, string? doctorNotes = null, CancellationToken cancellationToken = default);
    Task MarkAsNoShowAsync(Guid appointmentId, string markedBy, CancellationToken cancellationToken = default);

    // Seçilen doktor, hizmet ve tarihe göre müsait saat slotlarını döndürür
    Task<IReadOnlyList<AvailableSlotDto>> GetAvailableSlotsAsync(
        Guid doctorId, Guid serviceId, DateOnly date,
        CancellationToken cancellationToken = default);
}
