using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface IHealthcareServiceRepository : IRepository<HealthcareService>
{
    Task<IReadOnlyList<HealthcareService>> GetByDepartmentIdAsync(Guid departmentId, bool activeOnly = true, CancellationToken cancellationToken = default);
}
