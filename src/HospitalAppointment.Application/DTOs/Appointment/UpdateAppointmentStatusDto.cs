using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Application.DTOs.Appointment;

public record UpdateAppointmentStatusDto(
    AppointmentStatus NewStatus,
    string? Reason = null,
    string? Notes = null
);
