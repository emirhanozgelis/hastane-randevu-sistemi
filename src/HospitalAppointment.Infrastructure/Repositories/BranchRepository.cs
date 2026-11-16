using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Şube repository implementasyonu
public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(AppDbContext context) : base(context) { }

    // Hastaneye ait şubeleri bölüm sayısıyla getir
    public async Task<IReadOnlyList<Branch>> GetByHospitalIdAsync(
        Guid hospitalId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(b => b.HospitalId == hospitalId && b.IsActive)
            .Include(b => b.Departments)
            .Include(b => b.Hospital)
            .OrderBy(b => b.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Şubeyi bölümler ve çalışma saatleriyle birlikte getir
    public async Task<Branch?> GetWithDetailsAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(b => b.Departments)
            .Include(b => b.WorkingHours)
            .Include(b => b.Hospital)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == branchId, cancellationToken);
}
