namespace HospitalAppointment.Application.DTOs.HealthcareService;

public record HealthcareServiceDto(
    Guid Id,
    Guid DepartmentId,
    string DepartmentName,
    string Name,
    string? Description,
    int DurationMinutes,
    decimal Price,
    string Currency,
    bool IsActive
);

public record CreateHealthcareServiceDto(
    Guid DepartmentId,
    string Name,
    int DurationMinutes,
    decimal Price,
    string Currency = "TRY",
    string? Description = null
);

public record UpdateHealthcareServiceDto(
    string Name,
    int DurationMinutes,
    decimal Price,
    string Currency,
    string? Description = null
);
