using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IReadOnlyList<Department>> GetByBranchIdAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<Department?> GetWithServicesAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
