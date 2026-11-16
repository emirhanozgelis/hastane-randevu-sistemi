namespace HospitalAppointment.Application.DTOs.Branch;

public record BranchDto(
    Guid Id,
    Guid HospitalId,
    string HospitalName,
    string Name,
    string FullAddress,
    string PhoneNumber,
    string? Email,
    bool IsActive,
    int DepartmentCount
);

public record CreateBranchDto(
    Guid HospitalId,
    string Name,
    string Street,
    string District,
    string City,
    string PostalCode,
    string PhoneNumber,
    string? Email = null,
    string Country = "Türkiye"
);

public record UpdateBranchDto(
    string Name,
    string Street,
    string District,
    string City,
    string PostalCode,
    string PhoneNumber,
    string? Email = null,
    string Country = "Türkiye"
);
