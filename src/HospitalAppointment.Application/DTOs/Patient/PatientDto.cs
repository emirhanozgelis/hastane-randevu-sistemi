using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Application.DTOs.Patient;

public record PatientDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    string Email,
    string PhoneNumber,
    DateOnly DateOfBirth,
    int Age,
    Gender Gender,
    BloodType BloodType,
    string MedicalRecordNumber,
    string? Allergies,
    string? ChronicDiseases
);
