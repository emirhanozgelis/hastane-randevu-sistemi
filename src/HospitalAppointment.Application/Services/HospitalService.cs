using HospitalAppointment.Application.DTOs.Branch;
using HospitalAppointment.Application.DTOs.Hospital;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Application.Services;

public sealed class HospitalService : IHospitalService
{
    private readonly IHospitalRepository _hospitalRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HospitalService> _logger;

    public HospitalService(
        IHospitalRepository hospitalRepository,
        IUnitOfWork unitOfWork,
        ILogger<HospitalService> logger)
    {
        _hospitalRepository = hospitalRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<HospitalDto> CreateHospitalAsync(
        CreateHospitalDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Hastane oluşturuluyor — Ad: {Name}", dto.Name);

        var address = Address.Of(dto.Street, dto.District, dto.City, dto.PostalCode, dto.Country);
        var hospital = new Hospital(dto.Name, address, dto.TaxNumber, dto.ContactEmail);

        await _hospitalRepository.AddAsync(hospital, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Hastane oluşturuldu — Id: {HospitalId}", hospital.Id);

        return MapToDto(hospital);
    }

    public async Task<HospitalDto?> GetHospitalAsync(
        Guid hospitalId,
        CancellationToken cancellationToken = default)
    {
        var hospital = await _hospitalRepository.GetWithBranchesAsync(hospitalId, cancellationToken);
        return hospital is null ? null : MapToDto(hospital);
    }

    public async Task<IReadOnlyList<HospitalDto>> GetAllHospitalsAsync(
        CancellationToken cancellationToken = default)
    {
        var hospitals = await _hospitalRepository.GetAllAsync(cancellationToken);
        return hospitals.Select(MapToDto).ToList().AsReadOnly();
    }

    public async Task<HospitalDto> UpdateHospitalAsync(
        Guid hospitalId,
        UpdateHospitalDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Hastane güncelleniyor — Id: {HospitalId}", hospitalId);

        var hospital = await _hospitalRepository.GetByIdAsync(hospitalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Hastane bulunamadı: {hospitalId}");

        var address = Address.Of(dto.Street, dto.District, dto.City, dto.PostalCode, dto.Country);
        hospital.Update(dto.Name, address, dto.ContactEmail, dto.Website);

        await _hospitalRepository.UpdateAsync(hospital, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(hospital);
    }

    public async Task DeactivateHospitalAsync(
        Guid hospitalId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Hastane devre dışı bırakılıyor — Id: {HospitalId}", hospitalId);

        var hospital = await _hospitalRepository.GetByIdAsync(hospitalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Hastane bulunamadı: {hospitalId}");

        hospital.Deactivate();

        await _hospitalRepository.UpdateAsync(hospital, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateHospitalAsync(
        Guid hospitalId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Hastane aktif hale getiriliyor — Id: {HospitalId}", hospitalId);

        var hospital = await _hospitalRepository.GetByIdAsync(hospitalId, cancellationToken)
            ?? throw new KeyNotFoundException($"Hastane bulunamadı: {hospitalId}");

        hospital.Activate();

        await _hospitalRepository.UpdateAsync(hospital, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static HospitalDto MapToDto(Hospital h) => new(
        Id: h.Id,
        Name: h.Name,
        Street: h.Address.Street,
        District: h.Address.District,
        City: h.Address.City,
        PostalCode: h.Address.PostalCode,
        Country: h.Address.Country,
        FullAddress: h.Address.GetFullAddress(),
        TaxNumber: h.TaxNumber,
        ContactEmail: h.ContactEmail,
        Website: h.Website,
        IsActive: h.IsActive,
        BranchCount: h.Branches.Count,
        Branches: h.Branches.Select(b => new BranchDto(
            Id: b.Id,
            HospitalId: b.HospitalId,
            HospitalName: h.Name,
            Name: b.Name,
            FullAddress: b.Address.GetFullAddress(),
            PhoneNumber: b.PhoneNumber,
            Email: b.Email,
            IsActive: b.IsActive,
            DepartmentCount: b.Departments.Count)).ToList().AsReadOnly());
}
