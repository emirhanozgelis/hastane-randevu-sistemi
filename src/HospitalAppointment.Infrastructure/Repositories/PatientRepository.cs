using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Hasta repository implementasyonu
public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(AppDbContext context) : base(context) { }

    // Kullanıcı ID'sine göre hasta getir
    public async Task<Patient?> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

    // E-posta adresine göre hasta getir
    public async Task<Patient?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .FirstOrDefaultAsync(p => p.Email == email.ToLowerInvariant(), cancellationToken);

    // Hastayı randevularıyla birlikte getir — AsSplitQuery ile Cartesian explosion önlenir
    public async Task<Patient?> GetWithAppointmentsAsync(
        Guid patientId,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.Appointments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
}
