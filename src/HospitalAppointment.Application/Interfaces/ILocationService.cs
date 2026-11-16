using HospitalAppointment.Application.DTOs.Location;

namespace HospitalAppointment.Application.Interfaces;

public interface ILocationService
{
    Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DistrictDto>> GetDistrictsByCityAsync(Guid cityId, CancellationToken cancellationToken = default);
}
