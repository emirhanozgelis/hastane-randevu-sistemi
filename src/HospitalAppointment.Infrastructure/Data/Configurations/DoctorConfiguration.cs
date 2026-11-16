using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Doktor entity EF Core yapılandırması — TPH (Table Per Hierarchy) kalıtım stratejisi
public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        // TPH: Doktorlar kendi tablosunda saklanır
        builder.ToTable("Doctors");

        builder.Property(d => d.Specialization)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Biography)
            .HasMaxLength(2000);

        builder.Property(d => d.UserId)
            .IsRequired()
            .HasMaxLength(450);

        // Person ve Employee'den kalıtılan alanlar
        builder.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(d => d.LastName).IsRequired().HasMaxLength(100);
        builder.Property(d => d.Email).IsRequired().HasMaxLength(200);
        builder.Property(d => d.PhoneNumber).HasMaxLength(20);
        builder.Property(d => d.EmployeeNumber).IsRequired().HasMaxLength(50);
        builder.Property(d => d.IsActive).IsRequired();

        // Adres isteğe bağlı sahipli entity
        builder.OwnsOne(d => d.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(300);
            addressBuilder.Property(a => a.District).HasColumnName("Address_District").HasMaxLength(150);
            addressBuilder.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
            addressBuilder.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            addressBuilder.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
        });

        // Müsaitlik takvimi ile bire-çok ilişki — _availabilities backing field
        builder.HasMany(d => d.Availabilities)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Randevularla bire-çok ilişki — _appointments backing field
        builder.HasMany(d => d.Appointments)
            .WithOne(a => a.Doctor)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doktor-Hizmet çoktan-çoğa ilişki — ara tablo üzerinden
        builder.HasMany(d => d.HealthcareServices)
            .WithMany()
            .UsingEntity(j => j.ToTable("DoctorHealthcareServices"));

        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt);
    }
}
