using HospitalAppointment.Application.DTOs.Branch;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Application.Services;

public sealed class BranchService : IBranchService
{
    private readonly IBranchRepository _branchRepository;
    private readonly IHospitalRepository _hospitalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BranchService> _logger;

    public BranchService(
        IBranchRepository branchRepository,
        IHospitalRepository hospitalRepository,
        IUnitOfWork unitOfWork,
        ILogger<BranchService> logger)
    {
        _branchRepository = branchRepository;
        _hospitalRepository = hospitalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BranchDto> CreateBranchAsync(
        CreateBranchDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Şube oluşturuluyor — Hastane: {HospitalId}, Ad: {Name}", dto.HospitalId, dto.Name);

        var hospital = await _hospitalRepository.GetByIdAsync(dto.HospitalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Hastane bulunamadı: {dto.HospitalId}");

        var address = Address.Of(dto.Street, dto.District, dto.City, dto.PostalCode, dto.Country);
        var branch = new Branch(dto.HospitalId, dto.Name, address, dto.PhoneNumber, dto.Email);

        await _branchRepository.AddAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Şube oluşturuldu — Id: {BranchId}", branch.Id);

        return MapToDto(branch, hospital.Name);
    }

    public async Task<BranchDto?> GetBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var branch = await _branchRepository.GetWithDetailsAsync(branchId, cancellationToken);
        if (branch is null) return null;

        var hospitalName = branch.Hospital?.Name ?? string.Empty;
        return MapToDto(branch, hospitalName);
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesByHospitalAsync(
        Guid hospitalId,
        CancellationToken cancellationToken = default)
    {
        var hospital = await _hospitalRepository.GetByIdAsync(hospitalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Hastane bulunamadı: {hospitalId}");

        var branches = await _branchRepository.GetByHospitalIdAsync(hospitalId, cancellationToken);
        return branches.Select(b => MapToDto(b, hospital.Name)).ToList().AsReadOnly();
    }

    public async Task<BranchDto> UpdateBranchAsync(
        Guid branchId,
        UpdateBranchDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Şube güncelleniyor — Id: {BranchId}", branchId);

        var branch = await _branchRepository.GetWithDetailsAsync(branchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Şube bulunamadı: {branchId}");

        var address = Address.Of(dto.Street, dto.District, dto.City, dto.PostalCode, dto.Country);
        branch.Update(dto.Name, address, dto.PhoneNumber, dto.Email);

        await _branchRepository.UpdateAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(branch, branch.Hospital?.Name ?? string.Empty);
    }

    public async Task DeactivateBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Şube devre dışı bırakılıyor — Id: {BranchId}", branchId);

        var branch = await _branchRepository.GetByIdAsync(branchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Şube bulunamadı: {branchId}");

        branch.Deactivate();

        await _branchRepository.UpdateAsync(branch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static BranchDto MapToDto(Branch b, string hospitalName) => new(
        Id: b.Id,
        HospitalId: b.HospitalId,
        HospitalName: hospitalName,
        Name: b.Name,
        FullAddress: b.Address.GetFullAddress(),
        PhoneNumber: b.PhoneNumber,
        Email: b.Email,
        IsActive: b.IsActive,
        DepartmentCount: b.Departments.Count);
}
