using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Bölüm repository implementasyonu
public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(AppDbContext context) : base(context) { }

    // Şubeye ait aktif bölümleri, doktor ve hizmet sayısıyla getir
    public async Task<IReadOnlyList<Department>> GetByBranchIdAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(d => d.BranchId == branchId && d.IsActive)
            .Include(d => d.HealthcareServices)
            .Include(d => d.Doctors)
            .Include(d => d.Branch)
            .OrderBy(d => d.Name)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Bölümü sağlık hizmetleri, doktorlar ve şubeyle birlikte getir
    public async Task<Department?> GetWithServicesAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(d => d.HealthcareServices)
            .Include(d => d.Doctors)
            .Include(d => d.Branch)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == departmentId, cancellationToken);
}
