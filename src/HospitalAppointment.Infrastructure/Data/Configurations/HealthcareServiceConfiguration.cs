using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Sağlık hizmeti entity EF Core yapılandırması
public class HealthcareServiceConfiguration : IEntityTypeConfiguration<HealthcareService>
{
    public void Configure(EntityTypeBuilder<HealthcareService> builder)
    {
        builder.ToTable("HealthcareServices");

        builder.HasKey(hs => hs.Id);

        builder.Property(hs => hs.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(hs => hs.Description)
            .HasMaxLength(1000);

        builder.Property(hs => hs.IsActive)
            .IsRequired();

        // Para değer nesnesi sahipli entity — hassasiyet ve para birimi
        builder.OwnsOne(hs => hs.Price, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                .HasColumnName("Price_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(p => p.Currency)
                .HasColumnName("Price_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // TimeSpan tick cinsinden long olarak saklanır
        builder.Property(hs => hs.Duration)
            .HasConversion(
                ts => ts.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .HasColumnName("Duration");

        builder.Property(hs => hs.CreatedAt).IsRequired();
        builder.Property(hs => hs.UpdatedAt);
    }
}
