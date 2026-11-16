using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Application.DTOs.Appointment;

public record AppointmentDto(
    Guid Id,
    Guid HospitalId,
    string HospitalName,
    Guid BranchId,
    string BranchName,
    Guid DepartmentId,
    string DepartmentName,
    Guid DoctorId,
    string DoctorName,
    string DoctorSpecialization,
    Guid PatientId,
    string PatientName,
    Guid HealthcareServiceId,
    string ServiceName,
    DateOnly AppointmentDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    decimal Price,
    string Currency,
    AppointmentStatus Status,
    string StatusDisplay,
    string? PatientNotes,
    string? DoctorNotes,
    string? RejectionReason,
    DateTime CreatedAt
);
