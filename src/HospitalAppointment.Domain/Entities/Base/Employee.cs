using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Domain.Entities.Base;

// Soyut çalışan sınıfı — Person'dan kalıtım alır, Doctor ve MedicalStaff için taban
public abstract class Employee : Person
{
    public string EmployeeNumber { get; protected set; }
    public DateOnly HireDate { get; protected set; }
    public bool IsActive { get; protected set; }
    public Guid BranchId { get; protected set; }

    // EF Core için parametresiz constructor
    protected Employee() { EmployeeNumber = string.Empty; }

    protected Employee(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateOnly dateOfBirth,
        Gender gender,
        string employeeNumber,
        DateOnly hireDate,
        Guid branchId)
        : base(firstName, lastName, email, phoneNumber, dateOfBirth, gender)
    {
        if (string.IsNullOrWhiteSpace(employeeNumber))
            throw new ArgumentException("Personel numarası boş olamaz.", nameof(employeeNumber));
        if (branchId == Guid.Empty)
            throw new ArgumentException("Şube kimliği geçersiz.", nameof(branchId));

        EmployeeNumber = employeeNumber.Trim();
        HireDate = hireDate;
        IsActive = true;
        BranchId = branchId;
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

    public void TransferToBranch(Guid newBranchId)
    {
        if (newBranchId == Guid.Empty)
            throw new ArgumentException("Şube kimliği geçersiz.", nameof(newBranchId));
        BranchId = newBranchId;
        MarkUpdated();
    }

    // Alt sınıflar bu metotu ezecek — polimorfizm
    public abstract decimal CalculateConsultationFee();
}
