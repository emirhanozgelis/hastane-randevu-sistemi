using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Application.DTOs.Doctor;

public record CreateDoctorDto(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    Gender Gender,
    string EmployeeNumber,
    DateOnly HireDate,
    Guid BranchId,
    Guid DepartmentId,
    string Specialization,
    string LicenseNumber,
    string Password,
    string? Biography = null
);
