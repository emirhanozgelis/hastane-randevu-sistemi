using HospitalAppointment.Application.DTOs.Branch;

namespace HospitalAppointment.Application.DTOs.Hospital;

public record HospitalDto(
    Guid Id,
    string Name,
    string Street,
    string District,
    string City,
    string PostalCode,
    string Country,
    string FullAddress,
    string? TaxNumber,
    string? ContactEmail,
    string? Website,
    bool IsActive,
    int BranchCount,
    IReadOnlyList<BranchDto>? Branches = null
);

public record CreateHospitalDto(
    string Name,
    string Street,
    string District,
    string City,
    string PostalCode,
    string Country = "Türkiye",
    string? TaxNumber = null,
    string? ContactEmail = null,
    string? Website = null
);

public record UpdateHospitalDto(
    string Name,
    string Street,
    string District,
    string City,
    string PostalCode,
    string Country,
    string? ContactEmail = null,
    string? Website = null
);
