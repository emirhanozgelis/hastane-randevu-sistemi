using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Hastane repository implementasyonu
public class HospitalRepository : Repository<Hospital>, IHospitalRepository
{
    public HospitalRepository(AppDbContext context) : base(context) { }

    // Aktif hastaneleri şube sayısıyla birlikte getir
    public async Task<IReadOnlyList<Hospital>> GetActiveHospitalsAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(h => h.IsActive)
            .Include(h => h.Branches)
            .OrderBy(h => h.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Tüm hastaneleri şube sayısıyla getir
    public override async Task<IReadOnlyList<Hospital>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(h => h.Branches)
            .OrderBy(h => h.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // Hastaneyi şubeleriyle birlikte getir
    public async Task<Hospital?> GetWithBranchesAsync(
        Guid hospitalId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(h => h.Branches)
            .AsSplitQuery()
            .FirstOrDefaultAsync(h => h.Id == hospitalId, cancellationToken);
}
