using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Randevu durum geçmişi entity EF Core yapılandırması
public class AppointmentStatusHistoryConfiguration : IEntityTypeConfiguration<AppointmentStatusHistory>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusHistory> builder)
    {
        builder.ToTable("AppointmentStatusHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.AppointmentId).IsRequired();

        // Durum int olarak saklanır
        builder.Property(h => h.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(h => h.ChangedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(h => h.Reason).HasMaxLength(500);

        builder.Property(h => h.ChangedAt).IsRequired();

        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.UpdatedAt);
    }
}
