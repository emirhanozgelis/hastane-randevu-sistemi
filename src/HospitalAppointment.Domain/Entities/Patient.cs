using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Domain.Entities;

// Hasta entity'si — Person'dan kalıtım alır
public class Patient : Person
{
    public string MedicalRecordNumber { get; private set; }
    public BloodType BloodType { get; private set; }
    public string? Allergies { get; private set; }
    public string? ChronicDiseases { get; private set; }
    public string UserId { get; private set; }

    private readonly List<Appointment> _appointments = [];
    public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

    // EF Core için — base Person() parametresiz constructor'ı çağırır
    private Patient() { MedicalRecordNumber = ""; UserId = ""; }

    public Patient(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        DateOnly dateOfBirth,
        Gender gender,
        string userId,
        BloodType bloodType = BloodType.Unknown)
        : base(firstName, lastName, email, phoneNumber, dateOfBirth, gender)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));

        UserId = userId;
        BloodType = bloodType;
        MedicalRecordNumber = GenerateMedicalRecordNumber();
    }

    // Çok biçimlilik (Polymorphism) — Person.GetDisplayRole() eziliyor
    public override string GetDisplayRole() => "Hasta";

    public void UpdateMedicalInfo(BloodType bloodType, string? allergies, string? chronicDiseases)
    {
        BloodType = bloodType;
        Allergies = allergies?.Trim();
        ChronicDiseases = chronicDiseases?.Trim();
        MarkUpdated();
    }

    private static string GenerateMedicalRecordNumber()
    {
        // Benzersiz hasta kayıt numarası: HAS + yıl + rastgele sayı
        return $"HAS{DateTime.UtcNow.Year}{Random.Shared.Next(100000, 999999)}";
    }
}
