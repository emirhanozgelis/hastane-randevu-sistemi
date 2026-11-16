using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalAppointment.Infrastructure.Data.Configurations;

// Randevu entity EF Core yapılandırması
public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.HospitalId).IsRequired();
        builder.Property(a => a.BranchId).IsRequired();
        builder.Property(a => a.DepartmentId).IsRequired();
        builder.Property(a => a.DoctorId).IsRequired();
        builder.Property(a => a.PatientId).IsRequired();
        builder.Property(a => a.HealthcareServiceId).IsRequired();

        // Npgsql 10'da DateOnly ve TimeOnly doğrudan desteklenir
        builder.Property(a => a.AppointmentDate).IsRequired();
        builder.Property(a => a.StartTime).IsRequired();
        builder.Property(a => a.EndTime).IsRequired();

        // Anlık fiyat snapshot — para değer nesnesi
        builder.OwnsOne(a => a.SnapshotPrice, priceBuilder =>
        {
            priceBuilder.Property(p => p.Amount)
                .HasColumnName("SnapshotPrice_Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            priceBuilder.Property(p => p.Currency)
                .HasColumnName("SnapshotPrice_Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Anlık süre snapshot — tick cinsinden long
        builder.Property(a => a.SnapshotDuration)
            .HasConversion(
                ts => ts.Ticks,
                ticks => TimeSpan.FromTicks(ticks))
            .HasColumnName("SnapshotDuration");

        builder.Property(a => a.SnapshotServiceName)
            .IsRequired()
            .HasMaxLength(200);

        // Randevu durumu int olarak saklanır
        builder.Property(a => a.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.PatientNotes).HasMaxLength(1000);
        builder.Property(a => a.DoctorNotes).HasMaxLength(1000);
        builder.Property(a => a.RejectionReason).HasMaxLength(500);

        // Navigasyon ilişkileri — EF Core için açıkça tanımlanır
        builder.HasOne(a => a.Hospital)
            .WithMany()
            .HasForeignKey(a => a.HospitalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Branch)
            .WithMany()
            .HasForeignKey(a => a.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Department)
            .WithMany()
            .HasForeignKey(a => a.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.HealthcareService)
            .WithMany()
            .HasForeignKey(a => a.HealthcareServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Durum geçmişiyle bire-çok ilişki — private backing field belirtilmeli
        builder.HasMany(a => a.StatusHistory)
            .WithOne(h => h.Appointment)
            .HasForeignKey(h => h.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.StatusHistory).HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Tarih ve doktor bazlı sorgular için indeks
        builder.HasIndex(a => new { a.DoctorId, a.AppointmentDate });
        builder.HasIndex(a => new { a.PatientId, a.AppointmentDate });
        builder.HasIndex(a => a.Status);

        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt);
    }
}
