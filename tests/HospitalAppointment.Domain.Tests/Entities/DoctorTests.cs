using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using FluentAssertions;

namespace HospitalAppointment.Domain.Tests.Entities;

public class DoctorTests
{
    private static Doctor CreateDoctor()
    {
        return new Doctor(
            firstName: "Ahmet",
            lastName: "Yılmaz",
            email: "ahmet.yilmaz@hospital.com",
            phoneNumber: "5551234567",
            dateOfBirth: new DateOnly(1980, 5, 15),
            gender: Gender.Male,
            employeeNumber: "DOC001",
            hireDate: new DateOnly(2020, 1, 1),
            branchId: Guid.NewGuid(),
            departmentId: Guid.NewGuid(),
            specialization: "Kardiyoloji",
            licenseNumber: "LIC12345",
            userId: Guid.NewGuid().ToString()
        );
    }

    [Fact]
    public void NewDoctor_IsActiveByDefault()
    {
        var doctor = CreateDoctor();

        doctor.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var doctor = CreateDoctor();

        doctor.Deactivate();

        doctor.IsActive.Should().BeFalse();
    }

    [Fact]
    public void GetDisplayRole_IncludesSpecialization()
    {
        var doctor = CreateDoctor();

        var role = doctor.GetDisplayRole();

        role.Should().Contain("Dr.");
        role.Should().Contain("Kardiyoloji");
    }

    [Fact]
    public void CalculateConsultationFee_Cardiologist_Returns800()
    {
        var doctor = CreateDoctor(); // Kardiyoloji

        var fee = doctor.CalculateConsultationFee();

        fee.Should().Be(800m);
    }

    [Fact]
    public void CreateDoctor_WithEmptySpecialization_ThrowsArgumentException()
    {
        var act = () => new Doctor(
            "Ahmet", "Yılmaz", "test@test.com", "5551234567",
            new DateOnly(1980, 1, 1), Gender.Male,
            "DOC001", new DateOnly(2020, 1, 1),
            Guid.NewGuid(), Guid.NewGuid(),
            specialization: "",
            licenseNumber: "LIC123",
            userId: "user1");

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Uzmanlık alanı*");
    }

    [Fact]
    public void TransferToBranch_UpdatesBranchId()
    {
        var doctor = CreateDoctor();
        var newBranchId = Guid.NewGuid();

        doctor.TransferToBranch(newBranchId);

        doctor.BranchId.Should().Be(newBranchId);
    }

    [Fact]
    public void FullName_ReturnsFirstAndLastName()
    {
        var doctor = CreateDoctor();

        doctor.FullName.Should().Be("Ahmet Yılmaz");
    }
}
