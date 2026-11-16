namespace HospitalAppointment.Application.DTOs.Doctor;

public record DoctorAvailabilityDto(
    Guid Id,
    Guid DoctorId,
    string DoctorName,
    DateOnly Date,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable,
    bool IsSpecificDate,
    string? Notes
);

public record CreateDoctorAvailabilityDto(
    Guid DoctorId,
    DayOfWeek? DayOfWeek,
    DateOnly? SpecificDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsAvailable = true,
    string? Notes = null
);
