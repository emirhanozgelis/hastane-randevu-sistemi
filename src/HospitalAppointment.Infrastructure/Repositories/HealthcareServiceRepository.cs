using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Sağlık hizmeti repository implementasyonu
public class HealthcareServiceRepository : Repository<HealthcareService>, IHealthcareServiceRepository
{
    public HealthcareServiceRepository(AppDbContext context) : base(context) { }

    // Bölüme göre sağlık hizmetlerini getir, isteğe bağlı aktif filtresi
    public async Task<IReadOnlyList<HealthcareService>> GetByDepartmentIdAsync(
        Guid departmentId,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(hs => hs.DepartmentId == departmentId);

        if (activeOnly)
            query = query.Where(hs => hs.IsActive);

        return await query
            .OrderBy(hs => hs.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
