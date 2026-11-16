using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using FluentAssertions;

namespace HospitalAppointment.Domain.Tests.Entities;

public class PatientTests
{
    private static Patient CreatePatient()
    {
        return new Patient(
            firstName: "Ayşe",
            lastName: "Kaya",
            email: "ayse.kaya@example.com",
            phoneNumber: "5559876543",
            dateOfBirth: new DateOnly(1990, 3, 20),
            gender: Gender.Female,
            userId: Guid.NewGuid().ToString(),
            bloodType: BloodType.APositive
        );
    }

    [Fact]
    public void NewPatient_HasMedicalRecordNumber()
    {
        var patient = CreatePatient();

        patient.MedicalRecordNumber.Should().StartWith("HAS");
        patient.MedicalRecordNumber.Should().HaveLength(13); // HAS + 4 yıl + 6 rakam
    }

    [Fact]
    public void GetDisplayRole_ReturnsHasta()
    {
        var patient = CreatePatient();

        patient.GetDisplayRole().Should().Be("Hasta");
    }

    [Fact]
    public void UpdateMedicalInfo_UpdatesBloodType()
    {
        var patient = CreatePatient();

        patient.UpdateMedicalInfo(BloodType.ONegative, "Penisilin", "Diyabet");

        patient.BloodType.Should().Be(BloodType.ONegative);
        patient.Allergies.Should().Be("Penisilin");
        patient.ChronicDiseases.Should().Be("Diyabet");
    }

    [Fact]
    public void CreatePatient_WithEmptyUserId_ThrowsArgumentException()
    {
        var act = () => new Patient(
            "Ayşe", "Kaya", "test@test.com", "5551234567",
            new DateOnly(1990, 1, 1), Gender.Female,
            userId: "");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Kullanıcı kimliği*");
    }

    [Fact]
    public void Age_IsCalculatedCorrectly()
    {
        // 1990 doğumlu biri 2026'da 36 yaşında
        var patient = new Patient("Test", "User", "test@test.com", "5551234567",
            new DateOnly(1990, 1, 1), Gender.Male, "user1");

        patient.Age.Should().BeGreaterThanOrEqualTo(35);
    }
}
