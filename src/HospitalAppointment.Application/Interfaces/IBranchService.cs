using HospitalAppointment.Application.DTOs.Branch;

namespace HospitalAppointment.Application.Interfaces;

public interface IBranchService
{
    Task<BranchDto> CreateBranchAsync(CreateBranchDto dto, CancellationToken cancellationToken = default);
    Task<BranchDto?> GetBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BranchDto>> GetBranchesByHospitalAsync(Guid hospitalId, CancellationToken cancellationToken = default);
    Task<BranchDto> UpdateBranchAsync(Guid branchId, UpdateBranchDto dto, CancellationToken cancellationToken = default);
    Task DeactivateBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
}
