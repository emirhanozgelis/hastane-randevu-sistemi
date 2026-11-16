using HospitalAppointment.Application.DTOs.Department;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Application.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IBranchRepository branchRepository,
        IUnitOfWork unitOfWork,
        ILogger<DepartmentService> logger)
    {
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(
        CreateDepartmentDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bölüm oluşturuluyor — Şube: {BranchId}, Ad: {Name}", dto.BranchId, dto.Name);

        var branch = await _branchRepository.GetByIdAsync(dto.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Şube bulunamadı: {dto.BranchId}");

        var department = new Department(dto.BranchId, dto.Name, dto.Description);

        await _departmentRepository.AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Bölüm oluşturuldu — Id: {DepartmentId}", department.Id);

        return MapToDto(department, branch.Name);
    }

    public async Task<DepartmentDto?> GetDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetWithServicesAsync(departmentId, cancellationToken);
        if (department is null) return null;

        return MapToDto(department, department.Branch?.Name ?? string.Empty);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsByBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Şube bulunamadı: {branchId}");

        var departments = await _departmentRepository.GetByBranchIdAsync(branchId, cancellationToken);
        return departments.Select(d => MapToDto(d, branch.Name)).ToList().AsReadOnly();
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(
        Guid departmentId,
        UpdateDepartmentDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bölüm güncelleniyor — Id: {DepartmentId}", departmentId);

        var department = await _departmentRepository.GetWithServicesAsync(departmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bölüm bulunamadı: {departmentId}");

        department.Update(dto.Name, dto.Description);

        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(department, department.Branch?.Name ?? string.Empty);
    }

    public async Task DeactivateDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Bölüm devre dışı bırakılıyor — Id: {DepartmentId}", departmentId);

        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bölüm bulunamadı: {departmentId}");

        department.Deactivate();

        await _departmentRepository.UpdateAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static DepartmentDto MapToDto(Department d, string branchName) => new(
        Id: d.Id,
        BranchId: d.BranchId,
        BranchName: branchName,
        Name: d.Name,
        Description: d.Description,
        IsActive: d.IsActive,
        DoctorCount: d.Doctors.Count,
        ServiceCount: d.HealthcareServices.Count);
}
