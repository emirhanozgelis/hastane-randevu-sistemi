using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Exceptions;
using HospitalAppointment.Domain.ValueObjects;
using FluentAssertions;

namespace HospitalAppointment.Domain.Tests.Entities;

public class AppointmentTests
{
    private static Appointment CreateValidAppointment(DateOnly? date = null)
    {
        var appointmentDate = date ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        return new Appointment(
            hospitalId: Guid.NewGuid(),
            branchId: Guid.NewGuid(),
            departmentId: Guid.NewGuid(),
            doctorId: Guid.NewGuid(),
            patientId: Guid.NewGuid(),
            healthcareServiceId: Guid.NewGuid(),
            appointmentDate: appointmentDate,
            startTime: new TimeOnly(10, 0),
            endTime: new TimeOnly(10, 30),
            snapshotPrice: Money.Of(500m),
            snapshotDuration: TimeSpan.FromMinutes(30),
            snapshotServiceName: "Genel Muayene"
        );
    }

    [Fact]
    public void NewAppointment_HasPendingStatus()
    {
        var appointment = CreateValidAppointment();

        appointment.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public void NewAppointment_HasStatusHistory()
    {
        var appointment = CreateValidAppointment();

        appointment.StatusHistory.Should().HaveCount(1);
        appointment.StatusHistory.First().Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public void Confirm_FromPending_SetsConfirmedStatus()
    {
        var appointment = CreateValidAppointment();

        appointment.Confirm("doctor1");

        appointment.Status.Should().Be(AppointmentStatus.Confirmed);
        appointment.StatusHistory.Should().HaveCount(2);
    }

    [Fact]
    public void Reject_FromPending_SetsRejectedStatus()
    {
        var appointment = CreateValidAppointment();

        appointment.Reject("doctor1", "Doktor bu tarihte müsait değil.");

        appointment.Status.Should().Be(AppointmentStatus.Rejected);
        appointment.RejectionReason.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Reject_WithoutReason_ThrowsDomainException()
    {
        var appointment = CreateValidAppointment();

        var act = () => appointment.Reject("doctor1", "");

        act.Should().Throw<DomainException>()
            .WithMessage("*Red nedeni*");
    }

    [Fact]
    public void Cancel_FromConfirmed_SetsCancelledStatus()
    {
        var appointment = CreateValidAppointment();
        appointment.Confirm("doctor1");

        appointment.Cancel("patient1", "Kişisel sebep.");

        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Confirm_FromRejected_ThrowsInvalidStatusTransitionException()
    {
        var appointment = CreateValidAppointment();
        appointment.Reject("doctor1", "Müsait değil.");

        var act = () => appointment.Confirm("doctor1");

        act.Should().Throw<InvalidStatusTransitionException>();
    }

    [Fact]
    public void Complete_FromConfirmed_SetsCompletedStatus()
    {
        var appointment = CreateValidAppointment();
        appointment.Confirm("doctor1");

        appointment.Complete("doctor1", "Muayene tamamlandı.");

        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void MarkAsNoShow_FromConfirmed_SetsNoShowStatus()
    {
        var appointment = CreateValidAppointment();
        appointment.Confirm("doctor1");

        appointment.MarkAsNoShow("staff1");

        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void ConflictsWith_OverlappingTimes_ReturnsTrue()
    {
        var appointment = CreateValidAppointment();
        // 10:00-10:30 mevcut randevu, 10:15-10:45 yeni randevu → çakışma

        var conflicts = appointment.ConflictsWith(
            appointment.AppointmentDate,
            new TimeOnly(10, 15),
            new TimeOnly(10, 45));

        conflicts.Should().BeTrue();
    }

    [Fact]
    public void ConflictsWith_NonOverlappingTimes_ReturnsFalse()
    {
        var appointment = CreateValidAppointment();
        // 10:00-10:30 mevcut, 11:00-11:30 yeni → çakışma yok

        var conflicts = appointment.ConflictsWith(
            appointment.AppointmentDate,
            new TimeOnly(11, 0),
            new TimeOnly(11, 30));

        conflicts.Should().BeFalse();
    }

    [Fact]
    public void ConflictsWith_CancelledAppointment_ReturnsFalse()
    {
        var appointment = CreateValidAppointment();
        appointment.Cancel("patient1");

        var conflicts = appointment.ConflictsWith(
            appointment.AppointmentDate,
            new TimeOnly(10, 0),
            new TimeOnly(10, 30));

        conflicts.Should().BeFalse();
    }

    [Fact]
    public void CreateAppointment_WithPastDate_ThrowsDomainException()
    {
        var pastDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-1));

        var act = () => CreateValidAppointment(pastDate);

        act.Should().Throw<DomainException>()
            .WithMessage("*geçmiş*");
    }

    [Fact]
    public void CreateAppointment_SnapshotPrice_IsPreserved()
    {
        var price = Money.Of(750m, "TRY");
        var appointment = new Appointment(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            new TimeOnly(9, 0), new TimeOnly(9, 30),
            price, TimeSpan.FromMinutes(30), "Test Hizmet");

        appointment.SnapshotPrice.Should().Be(price);
        appointment.SnapshotPrice.Amount.Should().Be(750m);
    }
}
