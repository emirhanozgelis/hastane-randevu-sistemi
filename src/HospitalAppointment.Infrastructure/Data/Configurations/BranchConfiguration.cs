using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Şube entity EF Core yapılandırması
public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(b => b.Email)
            .HasMaxLength(200);

        builder.Property(b => b.IsActive)
            .IsRequired();

        // Şube adresi sahipli entity
        builder.OwnsOne(b => b.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Street)
                .HasColumnName("Address_Street")
                .HasMaxLength(300)
                .IsRequired();

            addressBuilder.Property(a => a.District)
                .HasColumnName("Address_District")
                .HasMaxLength(150)
                .IsRequired();

            addressBuilder.Property(a => a.City)
                .HasColumnName("Address_City")
                .HasMaxLength(100)
                .IsRequired();

            addressBuilder.Property(a => a.PostalCode)
                .HasColumnName("Address_PostalCode")
                .HasMaxLength(20)
                .IsRequired();

            addressBuilder.Property(a => a.Country)
                .HasColumnName("Address_Country")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Bölümlerle bire-çok ilişki
        builder.HasMany(b => b.Departments)
            .WithOne(d => d.Branch)
            .HasForeignKey(d => d.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Çalışma saatleriyle bire-çok ilişki
        builder.HasMany(b => b.WorkingHours)
            .WithOne(wh => wh.Branch)
            .HasForeignKey(wh => wh.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(b => b.CreatedAt).IsRequired();
        builder.Property(b => b.UpdatedAt);
    }
}
