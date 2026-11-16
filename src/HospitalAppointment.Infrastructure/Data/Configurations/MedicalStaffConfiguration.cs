using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Tıbbi personel entity EF Core yapılandırması
public class MedicalStaffConfiguration : IEntityTypeConfiguration<MedicalStaff>
{
    public void Configure(EntityTypeBuilder<MedicalStaff> builder)
    {
        builder.ToTable("MedicalStaff");

        builder.HasKey(ms => ms.Id);

        builder.Property(ms => ms.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(ms => ms.LastName).IsRequired().HasMaxLength(100);
        builder.Property(ms => ms.Email).IsRequired().HasMaxLength(200);
        builder.Property(ms => ms.PhoneNumber).HasMaxLength(20);
        builder.Property(ms => ms.EmployeeNumber).IsRequired().HasMaxLength(50);
        builder.Property(ms => ms.IsActive).IsRequired();

        builder.Property(ms => ms.StaffRole)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ms => ms.UserId)
            .IsRequired()
            .HasMaxLength(450);

        // Adres isteğe bağlı sahipli entity
        builder.OwnsOne(ms => ms.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street).HasColumnName("Address_Street").HasMaxLength(300);
            addressBuilder.Property(a => a.District).HasColumnName("Address_District").HasMaxLength(150);
            addressBuilder.Property(a => a.City).HasColumnName("Address_City").HasMaxLength(100);
            addressBuilder.Property(a => a.PostalCode).HasColumnName("Address_PostalCode").HasMaxLength(20);
            addressBuilder.Property(a => a.Country).HasColumnName("Address_Country").HasMaxLength(100);
        });

        builder.HasIndex(ms => ms.UserId).IsUnique();

        builder.Property(ms => ms.CreatedAt).IsRequired();
        builder.Property(ms => ms.UpdatedAt);
    }
}
