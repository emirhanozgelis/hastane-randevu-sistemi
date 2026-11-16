namespace HospitalAppointment.Application.DTOs.Department;

public record DepartmentDto(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string Name,
    string? Description,
    bool IsActive,
    int DoctorCount,
    int ServiceCount
);

public record CreateDepartmentDto(
    Guid BranchId,
    string Name,
    string? Description = null
);

public record UpdateDepartmentDto(
    string Name,
    string? Description = null
);
