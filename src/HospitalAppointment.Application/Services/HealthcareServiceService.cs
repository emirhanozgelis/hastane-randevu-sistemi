using HospitalAppointment.Application.DTOs.HealthcareService;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Application.Services;

public sealed class HealthcareServiceService : IHealthcareServiceService
{
    private readonly IHealthcareServiceRepository _serviceRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<HealthcareServiceService> _logger;

    public HealthcareServiceService(
        IHealthcareServiceRepository serviceRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        ILogger<HealthcareServiceService> logger)
    {
        _serviceRepository = serviceRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<HealthcareServiceDto> CreateServiceAsync(
        CreateHealthcareServiceDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sağlık hizmeti oluşturuluyor — Bölüm: {DepartmentId}, Ad: {Name}", dto.DepartmentId, dto.Name);

        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bölüm bulunamadı: {dto.DepartmentId}");

        var duration = TimeSpan.FromMinutes(dto.DurationMinutes);
        var price = Money.Of(dto.Price, dto.Currency);

        var service = new Domain.Entities.HealthcareService(
            dto.DepartmentId, dto.Name, duration, price, dto.Description);

        await _serviceRepository.AddAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Sağlık hizmeti oluşturuldu — Id: {ServiceId}", service.Id);

        return MapToDto(service, department.Name);
    }

    public async Task<HealthcareServiceDto?> GetServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service is null) return null;

        var departmentName = service.Department?.Name ?? string.Empty;
        return MapToDto(service, departmentName);
    }

    public async Task<IReadOnlyList<HealthcareServiceDto>> GetServicesByDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bölüm bulunamadı: {departmentId}");

        var services = await _serviceRepository.GetByDepartmentIdAsync(departmentId, activeOnly: false, cancellationToken);
        return services.Select(s => MapToDto(s, department.Name)).ToList().AsReadOnly();
    }

    public async Task<HealthcareServiceDto> UpdateServiceAsync(
        Guid serviceId,
        UpdateHealthcareServiceDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sağlık hizmeti güncelleniyor — Id: {ServiceId}", serviceId);

        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sağlık hizmeti bulunamadı: {serviceId}");

        var duration = TimeSpan.FromMinutes(dto.DurationMinutes);
        var price = Money.Of(dto.Price, dto.Currency);

        service.Update(dto.Name, duration, price, dto.Description);

        await _serviceRepository.UpdateAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(service, service.Department?.Name ?? string.Empty);
    }

    public async Task DeactivateServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sağlık hizmeti devre dışı bırakılıyor — Id: {ServiceId}", serviceId);

        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sağlık hizmeti bulunamadı: {serviceId}");

        service.Deactivate();

        await _serviceRepository.UpdateAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateServiceAsync(
        Guid serviceId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sağlık hizmeti aktif hale getiriliyor — Id: {ServiceId}", serviceId);

        var service = await _serviceRepository.GetByIdAsync(serviceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sağlık hizmeti bulunamadı: {serviceId}");

        service.Activate();

        await _serviceRepository.UpdateAsync(service, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static HealthcareServiceDto MapToDto(Domain.Entities.HealthcareService s, string departmentName) => new(
        Id: s.Id,
        DepartmentId: s.DepartmentId,
        DepartmentName: departmentName,
        Name: s.Name,
        Description: s.Description,
        DurationMinutes: (int)s.Duration.TotalMinutes,
        Price: s.Price.Amount,
        Currency: s.Price.Currency,
        IsActive: s.IsActive);
}
