using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Domain.Entities;

// Randevu durum geçmişi — Appointment ile Composition
public class AppointmentStatusHistory : BaseEntity
{
    public Guid AppointmentId { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string ChangedBy { get; private set; }
    public string? Reason { get; private set; }
    public DateTime ChangedAt { get; private set; }

    public Appointment? Appointment { get; private set; }

    private AppointmentStatusHistory() { ChangedBy = ""; }

    public AppointmentStatusHistory(Guid appointmentId, AppointmentStatus status, string changedBy, string? reason = null)
    {
        AppointmentId = appointmentId;
        Status = status;
        ChangedBy = changedBy?.Trim() ?? "system";
        Reason = reason?.Trim();
        ChangedAt = DateTime.UtcNow;
    }
}
