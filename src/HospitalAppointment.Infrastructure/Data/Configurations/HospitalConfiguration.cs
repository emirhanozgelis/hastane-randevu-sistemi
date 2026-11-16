using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Hastane entity EF Core yapılandırması
public class HospitalConfiguration : IEntityTypeConfiguration<Hospital>
{
    public void Configure(EntityTypeBuilder<Hospital> builder)
    {
        builder.ToTable("Hospitals");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.TaxNumber)
            .HasMaxLength(50);

        builder.Property(h => h.ContactEmail)
            .HasMaxLength(200);

        builder.Property(h => h.Website)
            .HasMaxLength(300);

        builder.Property(h => h.IsActive)
            .IsRequired();

        // Adres değer nesnesi sahipli entity olarak yapılandırılır
        builder.OwnsOne(h => h.Address, addressBuilder =>
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

        // Şubelerle bire-çok ilişki — cascade delete
        builder.HasMany(h => h.Branches)
            .WithOne(b => b.Hospital)
            .HasForeignKey(b => b.HospitalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.UpdatedAt);
    }
}
