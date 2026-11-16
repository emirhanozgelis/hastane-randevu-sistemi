using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface IBranchRepository : IRepository<Branch>
{
    Task<IReadOnlyList<Branch>> GetByHospitalIdAsync(Guid hospitalId, CancellationToken cancellationToken = default);
    Task<Branch?> GetWithDetailsAsync(Guid branchId, CancellationToken cancellationToken = default);
}
