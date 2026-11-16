namespace HospitalAppointment.Application.DTOs.Location;

public record CityDto(Guid Id, int PlateCode, string Name);
public record DistrictDto(Guid Id, Guid CityId, string Name);
