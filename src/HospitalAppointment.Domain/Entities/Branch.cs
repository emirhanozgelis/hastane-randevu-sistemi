using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.ValueObjects;

namespace HospitalAppointment.Domain.Entities;

// Şube entity'si — Hospital ile Composition ilişkisi
public class Branch : BaseEntity
{
    public Guid HospitalId { get; private set; }
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public string PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public bool IsActive { get; private set; }

    public Hospital? Hospital { get; private set; }

    private readonly List<Department> _departments = [];
    private readonly List<WorkingHours> _workingHours = [];

    public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();
    public IReadOnlyCollection<WorkingHours> WorkingHours => _workingHours.AsReadOnly();

    private Branch()
    {
        Name = "";
        PhoneNumber = "";
        Address = Address.Of("", "", "", "");
    }

    public Branch(Guid hospitalId, string name, Address address, string phoneNumber, string? email = null)
    {
        if (hospitalId == Guid.Empty)
            throw new ArgumentException("Hastane kimliği geçersiz.", nameof(hospitalId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Şube adı boş olamaz.", nameof(name));

        HospitalId = hospitalId;
        Name = name.Trim();
        Address = address ?? throw new ArgumentNullException(nameof(address));
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        Email = email?.Trim();
        IsActive = true;
    }

    public void Update(string name, Address address, string phoneNumber, string? email = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Şube adı boş olamaz.", nameof(name));

        Name = name.Trim();
        Address = address ?? throw new ArgumentNullException(nameof(address));
        PhoneNumber = phoneNumber?.Trim() ?? string.Empty;
        Email = email?.Trim();
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }

    // Belirli bir gün için çalışma saatlerini döndürür
    public WorkingHours? GetWorkingHoursForDay(DayOfWeek day)
        => _workingHours.FirstOrDefault(wh => wh.Day == day && !wh.IsClosed);
}
