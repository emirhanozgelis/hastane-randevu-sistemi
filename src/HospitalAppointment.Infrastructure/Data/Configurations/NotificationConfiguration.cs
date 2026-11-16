using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Bildirim entity EF Core yapılandırması
public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.IsRead).IsRequired();

        // Bildirim tipi int olarak saklanır
        builder.Property(n => n.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(n => n.RelatedAppointmentId);

        // Kullanıcı bazlı bildirim sorguları için indeks
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => new { n.UserId, n.IsRead });

        builder.Property(n => n.CreatedAt).IsRequired();
        builder.Property(n => n.UpdatedAt);
    }
}
