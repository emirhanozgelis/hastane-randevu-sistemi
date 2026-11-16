using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Hasta entity EF Core yapılandırması
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(200);
        builder.Property(p => p.PhoneNumber).HasMaxLength(20);

        builder.Property(p => p.MedicalRecordNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Kan grubu int olarak saklanır
        builder.Property(p => p.BloodType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Allergies).HasMaxLength(1000);
        builder.Property(p => p.ChronicDiseases).HasMaxLength(1000);

        builder.Property(p => p.UserId)
            .IsRequired()
            .HasMaxLength(450);

        // E-posta indeksi — hızlı arama
        builder.HasIndex(p => p.Email).IsUnique();
        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => p.MedicalRecordNumber).IsUnique();

        // Adres isteğe bağlı sahipli entity
        builder.OwnsOne(p => p.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(300);
            addressBuilder.Property(a => a.District).HasColumnName("Address_District").HasMaxLength(150);
            addressBuilder.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
            addressBuilder.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            addressBuilder.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
        });

        // Randevularla bire-çok ilişki — _appointments backing field
        builder.HasMany(p => p.Appointments)
            .WithOne(a => a.Patient)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt);
    }
}
