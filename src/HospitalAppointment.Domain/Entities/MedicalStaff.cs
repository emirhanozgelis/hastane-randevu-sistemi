using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Domain.Entities;

// Tıbbi personel entity'si — Employee'den kalıtım alır
public class MedicalStaff : Employee
{
    public string StaffRole { get; private set; }
    public Guid DepartmentId { get; private set; }
    public string UserId { get; private set; }

    public Department? Department { get; private set; }

    // EF Core için — Employee() parametresiz constructor zinciri üzerinden
    private MedicalStaff() { StaffRole = ""; UserId = ""; }

    public MedicalStaff(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateOnly dateOfBirth,
        Gender gender,
        string employeeNumber,
        DateOnly hireDate,
        Guid branchId,
        Guid departmentId,
        string staffRole,
        string userId)
        : base(firstName, lastName, email, phoneNumber, dateOfBirth, gender, employeeNumber, hireDate, branchId)
    {
        if (string.IsNullOrWhiteSpace(staffRole))
            throw new ArgumentException("Personel rolü boş olamaz.", nameof(staffRole));
        if (departmentId == Guid.Empty)
            throw new ArgumentException("Bölüm kimliği geçersiz.", nameof(departmentId));
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        StaffRole = staffRole.Trim();
        DepartmentId = departmentId;
        UserId = userId;
    }

    // Polimorfizm — Person.GetDisplayRole() eziliyor
    public override string GetDisplayRole() => $"Tıbbi Personel — {StaffRole}";

    // Polimorfizm — Employee.CalculateConsultationFee() eziliyor
    // Tıbbi personel doğrudan danışmanlık ücreti almaz
    public override decimal CalculateConsultationFee() => 0m;

    public void UpdateRole(string newRole)
    {
        if (string.IsNullOrWhiteSpace(newRole))
            throw new ArgumentException("Personel rolü boş olamaz.", nameof(newRole));
        StaffRole = newRole.Trim();
        MarkUpdated();
    }
}
