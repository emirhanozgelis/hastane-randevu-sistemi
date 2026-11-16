using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface IHospitalRepository : IRepository<Hospital>
{
    Task<IReadOnlyList<Hospital>> GetActiveHospitalsAsync(CancellationToken cancellationToken = default);
    Task<Hospital?> GetWithBranchesAsync(Guid hospitalId, CancellationToken cancellationToken = default);
}
