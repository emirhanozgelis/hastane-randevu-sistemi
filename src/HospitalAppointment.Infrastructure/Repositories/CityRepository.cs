using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

public class CityRepository : ICityRepository
{
    private readonly AppDbContext _context;

    public CityRepository(AppDbContext context) => _context = context;

    // Tüm illeri plaka sırasıyla getir
    public async Task<IReadOnlyList<City>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Cities
            .OrderBy(c => c.PlateCode)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    // İle ait ilçeleri alfabetik getir
    public async Task<IReadOnlyList<District>> GetDistrictsByCityIdAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
        => await _context.Districts
            .Where(d => d.CityId == cityId)
            .OrderBy(d => d.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<City?> GetByPlateCodeAsync(int plateCode, CancellationToken cancellationToken = default)
        => await _context.Cities
            .Include(c => c.Districts)
            .FirstOrDefaultAsync(c => c.PlateCode == plateCode, cancellationToken);
}
