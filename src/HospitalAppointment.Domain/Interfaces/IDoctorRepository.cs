using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface IDoctorRepository : IRepository<Doctor>
{
    Task<IReadOnlyList<Doctor>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<Doctor?> GetWithDetailsAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Doctor>> GetAvailableDoctorsAsync(Guid departmentId, DateOnly date, TimeOnly startTime, TimeOnly endTime, CancellationToken cancellationToken = default);
    Task<Doctor?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
