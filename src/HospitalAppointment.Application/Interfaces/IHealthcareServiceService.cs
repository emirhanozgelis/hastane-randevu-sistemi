using HospitalAppointment.Application.DTOs.HealthcareService;

namespace HospitalAppointment.Application.Interfaces;

public interface IHealthcareServiceService
{
    Task<HealthcareServiceDto> CreateServiceAsync(CreateHealthcareServiceDto dto, CancellationToken cancellationToken = default);
    Task<HealthcareServiceDto?> GetServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HealthcareServiceDto>> GetServicesByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<HealthcareServiceDto> UpdateServiceAsync(Guid serviceId, UpdateHealthcareServiceDto dto, CancellationToken cancellationToken = default);
    Task DeactivateServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task ActivateServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
