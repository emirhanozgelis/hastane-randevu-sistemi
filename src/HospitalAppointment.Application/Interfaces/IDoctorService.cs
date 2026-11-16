using HospitalAppointment.Application.DTOs.Doctor;

namespace HospitalAppointment.Application.Interfaces;

public interface IDoctorService
{
    Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto dto, CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<DoctorDto?> GetDoctorByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorDto>> GetDoctorsByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorDto>> GetDoctorsByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(Guid doctorId, CancellationToken cancellationToken = default);
    Task<DoctorAvailabilityDto> AddAvailabilityAsync(CreateDoctorAvailabilityDto dto, CancellationToken cancellationToken = default);
    Task DeleteAvailabilityAsync(Guid availabilityId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DoctorDto>> GetAvailableDoctorsAsync(Guid departmentId, DateOnly date, TimeOnly startTime, CancellationToken cancellationToken = default);
}
