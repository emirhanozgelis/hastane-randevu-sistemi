using HospitalAppointment.Application.DTOs.Department;

namespace HospitalAppointment.Application.Interfaces;

public interface IDepartmentService
{
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto dto, CancellationToken cancellationToken = default);
    Task<DepartmentDto?> GetDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<DepartmentDto> UpdateDepartmentAsync(Guid departmentId, UpdateDepartmentDto dto, CancellationToken cancellationToken = default);
    Task DeactivateDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
