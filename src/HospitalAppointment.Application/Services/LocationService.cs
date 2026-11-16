using HospitalAppointment.Application.DTOs.Location;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Interfaces;

namespace HospitalAppointment.Application.Services;

public sealed class LocationService : ILocationService
{
    private readonly ICityRepository _cityRepository;

    public LocationService(ICityRepository cityRepository)
        => _cityRepository = cityRepository;

    public async Task<IReadOnlyList<CityDto>> GetCitiesAsync(CancellationToken cancellationToken = default)
    {
        var cities = await _cityRepository.GetAllAsync(cancellationToken);
        return cities.Select(c => new CityDto(c.Id, c.PlateCode, c.Name)).ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<DistrictDto>> GetDistrictsByCityAsync(
        Guid cityId,
        CancellationToken cancellationToken = default)
    {
        var districts = await _cityRepository.GetDistrictsByCityIdAsync(cityId, cancellationToken);
        return districts.Select(d => new DistrictDto(d.Id, d.CityId, d.Name)).ToList().AsReadOnly();
    }
}
