using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface ICityRepository
{
    Task<IReadOnlyList<City>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<District>> GetDistrictsByCityIdAsync(Guid cityId, CancellationToken cancellationToken = default);
    Task<City?> GetByPlateCodeAsync(int plateCode, CancellationToken cancellationToken = default);
}
