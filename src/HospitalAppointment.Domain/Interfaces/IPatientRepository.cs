using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Patient?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Patient?> GetWithAppointmentsAsync(Guid patientId, CancellationToken cancellationToken = default);
}
