using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Randevu repository implementasyonu — özel sorgu metodları içerir
public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext context) : base(context) { }

    // Doktora ait randevuları getir, isteğe bağlı tarih filtresi
    public async Task<IReadOnlyList<Appointment>> GetByDoctorIdAsync(
        Guid doctorId,
        DateOnly? date = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(a => a.DoctorId == doctorId);

        if (date.HasValue)
            query = query.Where(a => a.AppointmentDate == date.Value);

        return await query
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // Hastaya ait randevuları getir
    public async Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Doktor çakışma kontrolü — NewStart < ExistingEnd AND NewEnd > ExistingStart
    public async Task<bool> HasDoctorConflictAsync(
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        // İptal ve reddedilen randevular çakışma sayılmaz
        var conflictStatuses = new[] { AppointmentStatus.Cancelled, AppointmentStatus.Rejected };

        var query = _dbSet.Where(a =>
            a.DoctorId == doctorId &&
            a.AppointmentDate == date &&
            !conflictStatuses.Contains(a.Status) &&
            a.StartTime < endTime &&
            a.EndTime > startTime);

        // Güncelleme senaryosu: mevcut randevu hariç tutulur
        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    // Hasta çakışma kontrolü — aynı mantık
    public async Task<bool> HasPatientConflictAsync(
        Guid patientId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var conflictStatuses = new[] { AppointmentStatus.Cancelled, AppointmentStatus.Rejected };

        var query = _dbSet.Where(a =>
            a.PatientId == patientId &&
            a.AppointmentDate == date &&
            !conflictStatuses.Contains(a.Status) &&
            a.StartTime < endTime &&
            a.EndTime > startTime);

        if (excludeId.HasValue)
            query = query.Where(a => a.Id != excludeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    // Filtre bazlı detaylı randevu listesi — AsSplitQuery ile Cartesian explosion önlenir
    public async Task<IReadOnlyList<Appointment>> GetWithDetailsAsync(
        Guid? patientId = null,
        Guid? doctorId = null,
        Guid? branchId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.Department)
            .Include(a => a.HealthcareService)
            .AsSplitQuery()
            .AsNoTracking();

        if (patientId.HasValue)
            query = query.Where(a => a.PatientId == patientId.Value);

        if (doctorId.HasValue)
            query = query.Where(a => a.DoctorId == doctorId.Value);

        if (branchId.HasValue)
            query = query.Where(a => a.BranchId == branchId.Value);

        return await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    // Tek randevuyu tüm detaylarıyla getir (salt okunur)
    public async Task<Appointment?> GetWithDetailsAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .Include(a => a.Hospital)
            .Include(a => a.Branch)
            .Include(a => a.Department)
            .Include(a => a.HealthcareService)
            .Include(a => a.StatusHistory)
            .AsSplitQuery()
            .FirstOrDefaultAsync(a => a.Id == appointmentId, cancellationToken);

    // Randevu durumunu doğrudan SQL ile güncelle — EF tracking sorunlarını atlatır
    public async Task UpdateStatusAsync(
        Guid appointmentId,
        Domain.Enums.AppointmentStatus status,
        string? doctorNotes,
        string? rejectionReason,
        CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE ""Appointments""
              SET ""Status""={0}, ""DoctorNotes""={1}, ""RejectionReason""={2}, ""UpdatedAt""={3}
              WHERE ""Id""={4}",
            (int)status, doctorNotes, rejectionReason, DateTime.UtcNow, appointmentId);
    }

    // Randevu geçmişine kayıt ekle
    public async Task AddStatusHistoryAsync(
        Guid appointmentId,
        Domain.Enums.AppointmentStatus status,
        string changedBy,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await _context.Database.ExecuteSqlRawAsync(
            @"INSERT INTO ""AppointmentStatusHistories""
              (""Id"", ""AppointmentId"", ""Status"", ""ChangedBy"", ""Reason"", ""ChangedAt"", ""CreatedAt"")
              VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6})",
            id, appointmentId, (int)status, changedBy, reason, now, now);
    }
}
