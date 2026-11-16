using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Bölüm entity EF Core yapılandırması
public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.IsActive)
            .IsRequired();

        // Sağlık hizmetleriyle bire-çok ilişki
        builder.HasMany(d => d.HealthcareServices)
            .WithOne(hs => hs.Department)
            .HasForeignKey(hs => hs.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Doktorlarla bire-çok ilişki (doktorun DepartmentId FK'sı üzerinden)
        builder.HasMany(d => d.Doctors)
            .WithOne(doc => doc.Department)
            .HasForeignKey(doc => doc.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tıbbi personelle bire-çok ilişki
        builder.HasMany(d => d.MedicalStaff)
            .WithOne(ms => ms.Department)
            .HasForeignKey(ms => ms.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(d => d.CreatedAt).IsRequired();
        builder.Property(d => d.UpdatedAt);
    }
}
