namespace HospitalAppointment.Application.DTOs.Appointment;

public record CreateAppointmentDto(
    Guid HospitalId,
    Guid BranchId,
    Guid DepartmentId,
    Guid DoctorId,
    Guid HealthcareServiceId,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    string? Notes
);
