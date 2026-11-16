using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Exceptions;

namespace HospitalAppointment.Domain.Entities;

// Doktor entity'si — Employee'den kalıtım alır
public class Doctor : Employee
{
    public string Specialization { get; private set; }
    public string LicenseNumber { get; private set; }
    public Guid DepartmentId { get; private set; }
    public string? Biography { get; private set; }
    public string UserId { get; private set; }

    private readonly List<DoctorAvailability> _availabilities = [];
    private readonly List<Appointment> _appointments = [];
    private readonly List<HealthcareService> _healthcareServices = [];

    public IReadOnlyCollection<DoctorAvailability> Availabilities => _availabilities.AsReadOnly();
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();
    public IReadOnlyCollection<HealthcareService> HealthcareServices => _healthcareServices.AsReadOnly();

    // Navigasyon özellikleri (EF Core)
    public Department? Department { get; private set; }
    public Branch? Branch { get; private set; }

    // EF Core için — base zinciri üzerinden çalışır
    private Doctor() { Specialization = ""; LicenseNumber = ""; UserId = ""; }

    public Doctor(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateOnly dateOfBirth,
        Gender gender,
        string employeeNumber,
        DateOnly hireDate,
        Guid branchId,
        Guid departmentId,
        string specialization,
        string licenseNumber,
        string userId)
        : base(firstName, lastName, email, phoneNumber, dateOfBirth, gender, employeeNumber, hireDate, branchId)
    {
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Uzmanlık alanı boş olamaz.", nameof(specialization));
        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("Lisans numarası boş olamaz.", nameof(licenseNumber));
        if (departmentId == Guid.Empty)
            throw new ArgumentException("Bölüm kimliği geçersiz.", nameof(departmentId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        Specialization = specialization.Trim();
        LicenseNumber = licenseNumber.Trim();
        DepartmentId = departmentId;
        UserId = userId;
    }

    // Polimorfizm — Employee ve Person'ın soyut metodları eziliyor
    public override string GetDisplayRole() => $"Dr. — {Specialization}";

    // Polimorfizm — Employee.CalculateConsultationFee() eziliyor
    public override decimal CalculateConsultationFee()
    {
        // Doktorun uzmanlığına göre temel muayene ücreti
        return Specialization.ToLowerInvariant() switch
        {
            var s when s.Contains("kardiyoloji") => 800m,
            var s when s.Contains("nöroloji") => 750m,
            var s when s.Contains("ortopedi") => 700m,
            _ => 500m
        };
    }

    public void UpdateBiography(string? biography)
    {
        Biography = biography?.Trim();
        MarkUpdated();
    }

    public void UpdateSpecialization(string specialization)
    {
        if (string.IsNullOrWhiteSpace(specialization))
            throw new ArgumentException("Uzmanlık alanı boş olamaz.", nameof(specialization));
        Specialization = specialization.Trim();
        MarkUpdated();
    }

    // Belirli tarih ve saat için doktorun müsait olup olmadığını kontrol eder
    public bool IsAvailableAt(DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        var dayOfWeek = date.DayOfWeek;

        // Özel tarih izni var mı kontrol et
        var specificDateOverride = _availabilities
            .FirstOrDefault(a => a.Date == date);

        if (specificDateOverride != null)
            return specificDateOverride.IsAvailable
                && startTime >= specificDateOverride.StartTime
                && endTime <= specificDateOverride.EndTime;

        // Haftalık program kontrolü
        var weeklySchedule = _availabilities
            .Where(a => a.Date == DateOnly.MinValue && a.DayOfWeek == dayOfWeek)
            .FirstOrDefault();

        if (weeklySchedule == null) return false;

        return weeklySchedule.IsAvailable
            && startTime >= weeklySchedule.StartTime
            && endTime <= weeklySchedule.EndTime;
    }

    public void ChangeDepartment(Guid departmentId)
    {
        if (departmentId == Guid.Empty)
            throw new ArgumentException("Bölüm kimliği geçersiz.", nameof(departmentId));
        DepartmentId = departmentId;
        MarkUpdated();
    }
}
