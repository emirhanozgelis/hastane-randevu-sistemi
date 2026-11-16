namespace HospitalAppointment.Application.DTOs.Doctor;

public record DoctorDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber,
    string Specialization,
    string LicenseNumber,
    Guid BranchId,
    string BranchName,
    Guid DepartmentId,
    string DepartmentName,
    string? Biography,
    bool IsActive
);
