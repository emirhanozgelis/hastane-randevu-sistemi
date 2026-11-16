using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Exceptions;
using HospitalAppointment.Domain.ValueObjects;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace HospitalAppointment.IntegrationTests;

// Gerçek PostgreSQL veritabanı üzerinde integration testleri.
// Her test kendi AppDbContext örneğini kullanır — tracking sorunlarını önler.
// Çalıştırmak: TEST_CONNECTION_STRING="..." dotnet test tests/HospitalAppointment.IntegrationTests
public class AppointmentIntegrationTests : IAsyncLifetime
{
    private static readonly string ConnectionString =
        Environment.GetEnvironmentVariable("TEST_CONNECTION_STRING")
        ?? "Host=localhost;Port=5432;Database=HospitalDB_Test;Username=postgres;Password=postgres;Include Error Detail=true";

    private bool _dbAvailable;

    // Her test kendi fresh context'ini oluşturur — tracking sorunlarını önler
    private AppDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options);

    public async Task InitializeAsync()
    {
        try
        {
            await using var ctx = CreateContext();
            await ctx.Database.EnsureCreatedAsync();
            _dbAvailable = true;
        }
        catch { _dbAvailable = false; }
    }

    public async Task DisposeAsync()
    {
        if (!_dbAvailable) return;
        try
        {
            await using var ctx = CreateContext();
            await ctx.Database.EnsureDeletedAsync();
        }
        catch { /* sessizce geç */ }
    }

    private void SkipIfUnavailable()
    {
        if (!_dbAvailable)
            throw new Exception("PostgreSQL test DB mevcut değil — TEST_CONNECTION_STRING ayarlayın.");
    }

    // Testlerde paylaşılan temel veri — her testte yeniden oluşturulur (UUID benzersiz)
    private async Task<(Hospital hospital, Branch branch, Department dept, HealthcareService service, Doctor doctor, Patient patient)>
        CreateBaseDataAsync()
    {
        await using var ctx = CreateContext();

        var hospital = new Hospital("Test Hastanesi", Address.Of("Cadde 1", "Kadıköy", "İstanbul", "34710"));
        var branch = new Branch(hospital.Id, "Merkez Şube", Address.Of("Cadde 1", "Kadıköy", "İstanbul", "34710"), "5551234567");
        var dept = new Department(branch.Id, "Test Bölümü");
        var service = new HealthcareService(dept.Id, "Test Muayenesi", TimeSpan.FromMinutes(30), Money.Of(500m));

        ctx.Hospitals.Add(hospital);
        ctx.Branches.Add(branch);
        ctx.Departments.Add(dept);
        await ctx.SaveChangesAsync();

        ctx.HealthcareServices.Add(service);
        await ctx.SaveChangesAsync();

        var doctor = new Doctor(
            "Test", "Doktor", $"doktor{Guid.NewGuid():N}@test.com", "5551234567",
            new DateOnly(1980, 1, 1), Gender.Male,
            $"DOC{Guid.NewGuid():N}", new DateOnly(2020, 1, 1),
            branch.Id, dept.Id, "Test Uzmanlık", $"LIC{Guid.NewGuid():N}",
            Guid.NewGuid().ToString());

        var patient = new Patient(
            "Test", "Hasta", $"hasta{Guid.NewGuid():N}@test.com", "5559876543",
            new DateOnly(1990, 5, 15), Gender.Female,
            Guid.NewGuid().ToString(), BloodType.APositive);

        ctx.Doctors.Add(doctor);
        ctx.Patients.Add(patient);
        await ctx.SaveChangesAsync();

        return (hospital, branch, dept, service, doctor, patient);
    }

    // ─── TEST 1: Hospital kayıt ───────────────────────────────────────────────

    [Fact]
    public async Task Hospital_SaveAndRetrieve_WorksCorrectly()
    {
        SkipIfUnavailable();
        var (hospital, _, _, _, _, _) = await CreateBaseDataAsync();

        await using var ctx = CreateContext();
        var saved = await ctx.Hospitals.FindAsync(hospital.Id);

        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Hastanesi");
        saved.Address.City.Should().Be("İstanbul");
        saved.IsActive.Should().BeTrue();
    }

    // ─── TEST 2: Money Value Object ───────────────────────────────────────────

    [Fact]
    public async Task Money_ValueObject_SavedAndRetrievedCorrectly()
    {
        SkipIfUnavailable();
        var (_, _, _, service, _, _) = await CreateBaseDataAsync();

        await using var ctx = CreateContext();
        var saved = await ctx.HealthcareServices.FindAsync(service.Id);

        saved!.Price.Amount.Should().Be(500m);
        saved.Price.Currency.Should().Be("TRY");
        saved.Duration.TotalMinutes.Should().Be(30);
    }

    // ─── TEST 3: Randevu oluşturma ────────────────────────────────────────────

    [Fact]
    public async Task Appointment_Create_SavedWithPendingStatus()
    {
        SkipIfUnavailable();
        var (hospital, branch, dept, service, doctor, patient) = await CreateBaseDataAsync();

        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var aptId = Guid.Empty;

        await using (var ctx = CreateContext())
        {
            var apt = new Appointment(
                hospital.Id, branch.Id, dept.Id,
                doctor.Id, patient.Id, service.Id,
                tomorrow, new TimeOnly(10, 0), new TimeOnly(10, 30),
                Money.Of(500m), TimeSpan.FromMinutes(30), "Test Muayenesi");

            ctx.Appointments.Add(apt);
            await ctx.SaveChangesAsync();
            aptId = apt.Id;
        }

        // Yeni context ile doğrula
        await using var verifyCtx = CreateContext();
        var saved = await verifyCtx.Appointments.FindAsync(aptId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(AppointmentStatus.Pending);
        saved.SnapshotPrice.Amount.Should().Be(500m);
    }

    // ─── TEST 4: Domain durum geçişi — Pending → Confirmed ───────────────────

    [Fact]
    public async Task Appointment_DomainConfirm_StatusChangesInMemory()
    {
        SkipIfUnavailable();
        var (hospital, branch, dept, service, doctor, patient) = await CreateBaseDataAsync();

        var tomorrow = DateOnly.FromDateTime(DateTime.Today.AddDays(2));

        // Domain katmanı testi — DB persistence değil, iş kuralı
        var apt = new Appointment(
            hospital.Id, branch.Id, dept.Id,
            doctor.Id, patient.Id, service.Id,
            tomorrow, new TimeOnly(11, 0), new TimeOnly(11, 30),
            Money.Of(500m), TimeSpan.FromMinutes(30), "Muayene");

        apt.Status.Should().Be(AppointmentStatus.Pending);
        apt.Confirm("doktor@test.com", "Onaylandı.");
        apt.Status.Should().Be(AppointmentStatus.Confirmed);
        apt.StatusHistory.Should().HaveCount(2); // Pending + Confirmed
    }

    // ─── TEST 5: Çakışma tespiti ─────────────────────────────────────────────

    [Fact]
    public async Task Appointment_ConflictDetection_OverlappingBlocksNew()
    {
        SkipIfUnavailable();
        var (hospital, branch, dept, service, doctor, patient) = await CreateBaseDataAsync();

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(5));

        await using (var ctx = CreateContext())
        {
            var apt = new Appointment(
                hospital.Id, branch.Id, dept.Id,
                doctor.Id, patient.Id, service.Id,
                date, new TimeOnly(10, 0), new TimeOnly(10, 30),
                Money.Of(500m), TimeSpan.FromMinutes(30), "Muayene 1");
            ctx.Appointments.Add(apt);
            await ctx.SaveChangesAsync();
        }

        await using var verifyCtx = CreateContext();
        // 10:15-10:45 aralığı mevcut 10:00-10:30 ile çakışır
        var conflictExists = await verifyCtx.Appointments.AnyAsync(a =>
            a.DoctorId == doctor.Id &&
            a.AppointmentDate == date &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected &&
            a.StartTime < new TimeOnly(10, 45) &&
            a.EndTime > new TimeOnly(10, 15));

        conflictExists.Should().BeTrue();
    }

    // ─── TEST 6: Çakışmayan randevu ──────────────────────────────────────────

    [Fact]
    public async Task Appointment_NonOverlapping_NoConflictDetected()
    {
        SkipIfUnavailable();
        var (hospital, branch, dept, service, doctor, patient) = await CreateBaseDataAsync();

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(6));

        await using (var ctx = CreateContext())
        {
            var apt = new Appointment(
                hospital.Id, branch.Id, dept.Id,
                doctor.Id, patient.Id, service.Id,
                date, new TimeOnly(9, 0), new TimeOnly(9, 30),
                Money.Of(500m), TimeSpan.FromMinutes(30), "Muayene 1");
            ctx.Appointments.Add(apt);
            await ctx.SaveChangesAsync();
        }

        await using var verifyCtx = CreateContext();
        // 11:00-11:30 — hiç çakışma yok
        var noConflict = await verifyCtx.Appointments.AnyAsync(a =>
            a.DoctorId == doctor.Id &&
            a.AppointmentDate == date &&
            a.Status != AppointmentStatus.Cancelled &&
            a.StartTime < new TimeOnly(11, 30) &&
            a.EndTime > new TimeOnly(11, 0));

        noConflict.Should().BeFalse();
    }

    // ─── TEST 7: Domain durum geçiş iş kuralları ─────────────────────────────

    [Fact]
    public async Task Appointment_InvalidStatusTransition_ThrowsException()
    {
        SkipIfUnavailable();
        var (hospital, branch, dept, service, doctor, patient) = await CreateBaseDataAsync();

        var date = DateOnly.FromDateTime(DateTime.Today.AddDays(8));
        var apt = new Appointment(
            hospital.Id, branch.Id, dept.Id,
            doctor.Id, patient.Id, service.Id,
            date, new TimeOnly(14, 0), new TimeOnly(14, 30),
            Money.Of(500m), TimeSpan.FromMinutes(30), "Test");

        // Reddet
        apt.Reject("doktor@test.com", "Müsait değil.");
        apt.Status.Should().Be(AppointmentStatus.Rejected);

        // Reddedilen → Onayla → InvalidStatusTransitionException
        var act = () => apt.Confirm("doktor@test.com");
        act.Should().Throw<InvalidStatusTransitionException>();
    }
}
