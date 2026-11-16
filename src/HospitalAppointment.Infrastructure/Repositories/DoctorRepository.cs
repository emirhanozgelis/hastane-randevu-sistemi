using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Doktor repository implementasyonu
public class DoctorRepository : Repository<Doctor>, IDoctorRepository
{
    public DoctorRepository(AppDbContext context) : base(context) { }

    // Bölüme göre aktif doktorları getir — Branch ve Department navigasyonu dahil
    public async Task<IReadOnlyList<Doctor>> GetByDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(d => d.DepartmentId == departmentId && d.IsActive)
            .Include(d => d.Branch)
            .Include(d => d.Department)
            .OrderBy(d => d.LastName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Şubeye göre aktif doktorları getir — Branch ve Department navigasyonu dahil
    public async Task<IReadOnlyList<Doctor>> GetByBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(d => d.BranchId == branchId && d.IsActive)
            .Include(d => d.Branch)
            .Include(d => d.Department)
            .OrderBy(d => d.LastName)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Doktoru tüm detaylarıyla getir — AsSplitQuery ile Cartesian explosion önlenir
    public async Task<Doctor?> GetWithDetailsAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(d => d.Availabilities)
            .Include(d => d.Department)
            .Include(d => d.Branch)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

    // Belirli tarih ve saat için müsait doktorları getir
    public async Task<IReadOnlyList<Doctor>> GetAvailableDoctorsAsync(
        Guid departmentId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        CancellationToken cancellationToken = default)
    {
        var dayOfWeek = date.DayOfWeek;

        // Müsait doktorlar: bölümde aktif olan ve o gün/saat için uygunluğu olan
        var doctors = await _dbSet
            .Where(d => d.DepartmentId == departmentId && d.IsActive)
            .Include(d => d.Availabilities)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        // Domain metodu üzerinden müsaitlik kontrolü yapılır
        return doctors
            .Where(d => d.IsAvailableAt(date, startTime, endTime))
            .ToList()
            .AsReadOnly();
    }

    // Kullanıcı ID'sine göre doktor getir — Branch ve Department dahil
    public async Task<Doctor?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(d => d.Branch)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId, cancellationToken);
}
