using HospitalAppointment.Application.DTOs.Hospital;

namespace HospitalAppointment.Application.Interfaces;

public interface IHospitalService
{
    Task<HospitalDto> CreateHospitalAsync(CreateHospitalDto dto, CancellationToken cancellationToken = default);
    Task<HospitalDto?> GetHospitalAsync(Guid hospitalId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<HospitalDto>> GetAllHospitalsAsync(CancellationToken cancellationToken = default);
    Task<HospitalDto> UpdateHospitalAsync(Guid hospitalId, UpdateHospitalDto dto, CancellationToken cancellationToken = default);
    Task DeactivateHospitalAsync(Guid hospitalId, CancellationToken cancellationToken = default);
    Task ActivateHospitalAsync(Guid hospitalId, CancellationToken cancellationToken = default);
}
