using HospitalAppointment.Application.DTOs.Appointment;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Application.Services;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;

namespace HospitalAppointment.Application.Tests.Services;

public class AppointmentServiceTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock = new();
    private readonly Mock<IDoctorRepository> _doctorRepoMock = new();
    private readonly Mock<IPatientRepository> _patientRepoMock = new();
    private readonly Mock<IHealthcareServiceRepository> _serviceRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<INotificationService> _notificationMock = new();
    private readonly Mock<ILogger<AppointmentService>> _loggerMock = new();

    // AppointmentService constructor parametrelerinin doğru sırası
    private AppointmentService CreateService() => new(
        _appointmentRepoMock.Object,
        _patientRepoMock.Object,
        _doctorRepoMock.Object,
        _serviceRepoMock.Object,
        _notificationMock.Object,
        _unitOfWorkMock.Object,
        _loggerMock.Object
    );

    private static Doctor CreateDoctor(Guid? branchId = null, Guid? departmentId = null)
    {
        return new Doctor(
            "Ahmet", "Yılmaz", "doktor@test.com", "5551234567",
            new DateOnly(1980, 1, 1), Gender.Male,
            "DOC001", new DateOnly(2020, 1, 1),
            branchId ?? Guid.NewGuid(),
            departmentId ?? Guid.NewGuid(),
            "Kardiyoloji", "LIC001",
            Guid.NewGuid().ToString());
    }

    private static Patient CreatePatient()
    {
        return new Patient(
            "Ayşe", "Kaya", "hasta@test.com", "5559876543",
            new DateOnly(1990, 1, 1), Gender.Female,
            Guid.NewGuid().ToString());
    }

    private static Domain.Entities.HealthcareService CreateHealthService(Guid departmentId)
    {
        return new Domain.Entities.HealthcareService(
            departmentId, "Kardiyoloji Muayenesi",
            TimeSpan.FromMinutes(30), Money.Of(500m));
    }

    [Fact]
    public async Task CreateAppointmentAsync_WithConflict_ThrowsDomainException()
    {
        // Doktorda çakışan randevu var
        var doctor = CreateDoctor();
        var patient = CreatePatient();
        var healthService = CreateHealthService(doctor.DepartmentId);
        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

        _doctorRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(doctor);
        _patientRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(patient);
        _serviceRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthService);
        // Advisory lock mock — test ortamında PostgreSQL yok
        _unitOfWorkMock.Setup(u => u.AcquireAppointmentLocksAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _appointmentRepoMock.Setup(r => r.HasDoctorConflictAsync(
                It.IsAny<Guid>(), It.IsAny<DateOnly>(),
                It.IsAny<TimeOnly>(), It.IsAny<TimeOnly>(),
                It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true); // Çakışma var

        var dto = new CreateAppointmentDto(
            Guid.NewGuid(), doctor.BranchId, doctor.DepartmentId,
            doctor.Id, healthService.Id, tomorrow, new TimeOnly(10, 0), null);

        var sut = CreateService();
        var act = async () => await sut.CreateAppointmentAsync(patient.Id, dto);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ConfirmAppointment_ValidId_ChangesStatusAndCallsRawSql()
    {
        var appointment = new Appointment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(10, 0), new TimeOnly(10, 30),
            Money.Of(500m), TimeSpan.FromMinutes(30), "Test Hizmet");

        _appointmentRepoMock.Setup(r => r.GetWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Raw SQL metodları mock'la
        _appointmentRepoMock.Setup(r => r.UpdateStatusAsync(
                It.IsAny<Guid>(), It.IsAny<AppointmentStatus>(),
                It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _appointmentRepoMock.Setup(r => r.AddStatusHistoryAsync(
                It.IsAny<Guid>(), It.IsAny<AppointmentStatus>(),
                It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.ClearTracker());

        var sut = CreateService();
        await sut.ConfirmAppointmentAsync(appointment.Id, "doctor1");

        // Domain kuralı çalıştı mı (Confirm geçişi doğru)
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);

        // Raw SQL metotları çağrıldı mı
        _appointmentRepoMock.Verify(r => r.UpdateStatusAsync(
            appointment.Id, AppointmentStatus.Confirmed,
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepoMock.Verify(r => r.AddStatusHistoryAsync(
            appointment.Id, AppointmentStatus.Confirmed,
            "doctor1", It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAppointmentAsync_NonExistentId_ReturnsNull()
    {
        _appointmentRepoMock.Setup(r => r.GetWithDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var sut = CreateService();
        var result = await sut.GetAppointmentAsync(Guid.NewGuid());

        result.Should().BeNull();
    }
}
