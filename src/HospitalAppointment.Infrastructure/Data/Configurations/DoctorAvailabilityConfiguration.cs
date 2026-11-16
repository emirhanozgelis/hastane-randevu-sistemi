using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Doktor müsaitlik takvimi EF Core yapılandırması
public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.ToTable("DoctorAvailabilities");

        builder.HasKey(da => da.Id);

        builder.Property(da => da.DoctorId).IsRequired();

        // Npgsql 10'da DateOnly doğrudan desteklenir
        builder.Property(da => da.Date).IsRequired();

        // DayOfWeek int olarak saklanır
        builder.Property(da => da.DayOfWeek)
            .HasConversion<int>()
            .IsRequired();

        // Npgsql 10'da TimeOnly doğrudan desteklenir
        builder.Property(da => da.StartTime).IsRequired();
        builder.Property(da => da.EndTime).IsRequired();

        builder.Property(da => da.IsAvailable).IsRequired();
        builder.Property(da => da.Notes).HasMaxLength(500);

        // Hızlı sorgu için indeks
        builder.HasIndex(da => new { da.DoctorId, da.Date });
        builder.HasIndex(da => new { da.DoctorId, da.DayOfWeek });

        builder.Property(da => da.CreatedAt).IsRequired();
        builder.Property(da => da.UpdatedAt);
    }
}
