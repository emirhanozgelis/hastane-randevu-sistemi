using HospitalAppointment.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Çalışma saatleri entity EF Core yapılandırması
public class WorkingHoursConfiguration : IEntityTypeConfiguration<WorkingHours>
{
    public void Configure(EntityTypeBuilder<WorkingHours> builder)
    {
        builder.ToTable("WorkingHours");

        builder.HasKey(wh => wh.Id);

        builder.Property(wh => wh.BranchId).IsRequired();

        // DayOfWeek int olarak saklanır
        builder.Property(wh => wh.Day)
            .HasConversion<int>()
            .IsRequired();

        // Npgsql 10'da TimeOnly doğrudan desteklenir
        builder.Property(wh => wh.OpenTime).IsRequired();
        builder.Property(wh => wh.CloseTime).IsRequired();

        builder.Property(wh => wh.IsClosed).IsRequired();

        // Şube ve gün kombinasyonu için indeks
        builder.HasIndex(wh => new { wh.BranchId, wh.Day });

        builder.Property(wh => wh.CreatedAt).IsRequired();
        builder.Property(wh => wh.UpdatedAt);
    }
}
