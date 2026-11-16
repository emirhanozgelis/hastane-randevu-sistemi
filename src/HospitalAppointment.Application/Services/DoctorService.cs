using HospitalAppointment.Application.DTOs.Doctor;
using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Exceptions;
using HospitalAppointment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Application.Services;

public sealed class DoctorService : IDoctorService
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IRepository<DoctorAvailability> _availabilityRepository;
    private readonly IAuthService _authService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(
        IDoctorRepository doctorRepository,
        IDepartmentRepository departmentRepository,
        IBranchRepository branchRepository,
        IRepository<DoctorAvailability> availabilityRepository,
        IAuthService authService,
        IUnitOfWork unitOfWork,
        ILogger<DoctorService> logger)
    {
        _doctorRepository = doctorRepository;
        _departmentRepository = departmentRepository;
        _branchRepository = branchRepository;
        _availabilityRepository = availabilityRepository;
        _authService = authService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DoctorDto> CreateDoctorAsync(
        CreateDoctorDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Doktor oluşturuluyor — {FirstName} {LastName}", dto.FirstName, dto.LastName);

        // Şube ve bölüm varlık kontrolü
        var branch = await _branchRepository.GetByIdAsync(dto.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Şube bulunamadı: {dto.BranchId}");

        var department = await _departmentRepository.GetByIdAsync(dto.DepartmentId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bölüm bulunamadı: {dto.DepartmentId}");

        // Identity kullanıcısını oluştur ve userId al
        var userId = await _authService.CreateEmployeeUserAsync(
            dto.Email, dto.FirstName, dto.LastName, dto.Password, "Doctor", cancellationToken);

        var doctor = new Doctor(
            dto.FirstName,
            dto.LastName,
            dto.Email,
            dto.PhoneNumber,
            dto.DateOfBirth,
            dto.Gender,
            dto.EmployeeNumber,
            dto.HireDate,
            dto.BranchId,
            dto.DepartmentId,
            dto.Specialization,
            dto.LicenseNumber,
            userId);

        if (dto.Biography is not null)
            doctor.UpdateBiography(dto.Biography);

        await _doctorRepository.AddAsync(doctor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Doktor oluşturuldu — Id: {DoctorId}", doctor.Id);

        return MapToDto(doctor, branch.Name, department.Name);
    }

    public async Task<DoctorDto?> GetDoctorAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetWithDetailsAsync(doctorId, cancellationToken);
        if (doctor is null) return null;

        return MapToDto(doctor,
            doctor.Branch?.Name ?? string.Empty,
            doctor.Department?.Name ?? string.Empty);
    }

    public async Task<DoctorDto?> GetDoctorByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetByUserIdAsync(userId, cancellationToken);
        if (doctor is null) return null;

        return MapToDto(doctor,
            doctor.Branch?.Name ?? string.Empty,
            doctor.Department?.Name ?? string.Empty);
    }

    public async Task<IReadOnlyList<DoctorDto>> GetDoctorsByDepartmentAsync(
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var doctors = await _doctorRepository.GetByDepartmentAsync(departmentId, cancellationToken);
        return doctors.Select(d => MapToDto(d,
            d.Branch?.Name ?? string.Empty,
            d.Department?.Name ?? string.Empty))
            .ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<DoctorDto>> GetDoctorsByBranchAsync(
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        var doctors = await _doctorRepository.GetByBranchAsync(branchId, cancellationToken);
        return doctors.Select(d => MapToDto(d,
            d.Branch?.Name ?? string.Empty,
            d.Department?.Name ?? string.Empty))
            .ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(
        Guid doctorId,
        CancellationToken cancellationToken = default)
    {
        var doctor = await _doctorRepository.GetWithDetailsAsync(doctorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Doktor bulunamadı: {doctorId}");

        return doctor.Availabilities
            .Select(a => MapAvailabilityToDto(a, doctor.FullName))
            .ToList().AsReadOnly();
    }

    public async Task<DoctorAvailabilityDto> AddAvailabilityAsync(
        CreateDoctorAvailabilityDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Doktor müsaitlik kaydı ekleniyor — Doktor: {DoctorId}", dto.DoctorId);

        var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId, cancellationToken)
            ?? throw new KeyNotFoundException($"Doktor bulunamadı: {dto.DoctorId}");

        DoctorAvailability availability;

        if (dto.SpecificDate.HasValue)
        {
            // Belirli tarih geçersizliği veya özel program
            availability = dto.IsAvailable
                ? DoctorAvailability.CreateDateOverride(
                    dto.DoctorId,
                    dto.SpecificDate.Value,
                    dto.StartTime,
                    dto.EndTime,
                    dto.IsAvailable,
                    dto.Notes)
                : DoctorAvailability.CreateLeaveDay(dto.DoctorId, dto.SpecificDate.Value, dto.Notes);
        }
        else if (dto.DayOfWeek.HasValue)
        {
            // Haftalık tekrarlayan program
            availability = DoctorAvailability.CreateWeeklySchedule(
                dto.DoctorId,
                dto.DayOfWeek.Value,
                dto.StartTime,
                dto.EndTime);
        }
        else
        {
            throw new DomainException(
                "Haftalık gün veya özel tarih belirtilmelidir.",
                "AVAILABILITY_DAY_REQUIRED");
        }

        await _availabilityRepository.AddAsync(availability, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapAvailabilityToDto(availability, doctor.FullName);
    }

    public async Task DeleteAvailabilityAsync(
        Guid availabilityId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Doktor müsaitlik kaydı siliniyor — Id: {AvailabilityId}", availabilityId);

        var availability = await _availabilityRepository.GetByIdAsync(availabilityId, cancellationToken)
            ?? throw new KeyNotFoundException($"Müsaitlik kaydı bulunamadı: {availabilityId}");

        await _availabilityRepository.DeleteAsync(availability, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DoctorDto>> GetAvailableDoctorsAsync(
        Guid departmentId,
        DateOnly date,
        TimeOnly startTime,
        CancellationToken cancellationToken = default)
    {
        // Hizmet süresi bilinmediğinden bitiş saati için 1 saat varsayılan kullanılır
        var endTime = startTime.Add(TimeSpan.FromHours(1));

        var doctors = await _doctorRepository.GetAvailableDoctorsAsync(
            departmentId, date, startTime, endTime, cancellationToken);

        return doctors.Select(d => MapToDto(d,
            d.Branch?.Name ?? string.Empty,
            d.Department?.Name ?? string.Empty))
            .ToList().AsReadOnly();
    }

    private static DoctorDto MapToDto(Doctor d, string branchName, string departmentName) => new(
        Id: d.Id,
        FirstName: d.FirstName,
        LastName: d.LastName,
        FullName: d.FullName,
        Email: d.Email,
        PhoneNumber: d.PhoneNumber,
        Specialization: d.Specialization,
        LicenseNumber: d.LicenseNumber,
        BranchId: d.BranchId,
        BranchName: branchName,
        DepartmentId: d.DepartmentId,
        DepartmentName: departmentName,
        Biography: d.Biography,
        IsActive: d.IsActive);

    private static DoctorAvailabilityDto MapAvailabilityToDto(DoctorAvailability a, string doctorName) => new(
        Id: a.Id,
        DoctorId: a.DoctorId,
        DoctorName: doctorName,
        Date: a.Date,
        DayOfWeek: a.DayOfWeek,
        StartTime: a.StartTime,
        EndTime: a.EndTime,
        IsAvailable: a.IsAvailable,
        IsSpecificDate: a.IsSpecificDate,
        Notes: a.Notes);
}
