using HospitalAppointment.Domain.Entities.Base;

namespace HospitalAppointment.Domain.Entities;

// Bölüm entity'si — Branch ile Composition, Doctor ve HealthcareService ile Association
public class Department : BaseEntity
{
    public Guid BranchId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public Branch? Branch { get; private set; }

    private readonly List<HealthcareService> _healthcareServices = [];
    private readonly List<Doctor> _doctors = [];
    private readonly List<MedicalStaff> _medicalStaff = [];

    public IReadOnlyCollection<HealthcareService> HealthcareServices => _healthcareServices.AsReadOnly();
    public IReadOnlyCollection<Doctor> Doctors => _doctors.AsReadOnly();
    public IReadOnlyCollection<MedicalStaff> MedicalStaff => _medicalStaff.AsReadOnly();

    private Department() { Name = ""; }

    public Department(Guid branchId, string name, string? description = null)
    {
        if (branchId == Guid.Empty)
            throw new ArgumentException("Şube kimliği geçersiz.", nameof(branchId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bölüm adı boş olamaz.", nameof(name));

        BranchId = branchId;
        Name = name.Trim();
        Description = description?.Trim();
        IsActive = true;
    }

    public void Update(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bölüm adı boş olamaz.", nameof(name));
        Name = name.Trim();
        Description = description?.Trim();
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
}
