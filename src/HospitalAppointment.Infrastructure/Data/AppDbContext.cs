using System.Reflection;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Data;

// Ana veritabanı bağlamı — IdentityDbContext ile EF Core entegrasyonu
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Hastane hiyerarşisi
    public DbSet<Hospital> Hospitals => Set<Hospital>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<HealthcareService> HealthcareServices => Set<HealthcareService>();

    // Personel — her biri kendi tablosunda (TPC)
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<MedicalStaff> MedicalStaff => Set<MedicalStaff>();

    // Randevu yönetimi
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentStatusHistory> AppointmentStatusHistories => Set<AppointmentStatusHistory>();

    // Takvim ve bildirimler
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<WorkingHours> WorkingHours => Set<WorkingHours>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Önce Identity yapılandırması çalıştırılır
        base.OnModelCreating(builder);

        // Assembly içindeki tüm IEntityTypeConfiguration sınıfları otomatik uygulanır
        // Doctor→"Doctors", Patient→"Patients", MedicalStaff→"MedicalStaff" ayrı tablolar
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
