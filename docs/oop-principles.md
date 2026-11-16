# OOP Prensipleri — Gerçek Kod Örnekleri

## 1. Kapsülleme (Encapsulation)

**Nerede:** Tüm entity sınıfları, özellikle `Appointment`, `Money`, `Doctor`

**Örnek — Appointment:**
```csharp
// Dışarıdan durum değiştirilemez, yalnızca kendi metotları üzerinden
public class Appointment : BaseEntity
{
    public AppointmentStatus Status { get; private set; }  // private setter

    public void Confirm(string confirmedBy, string? notes = null)
    {
        EnsureStatusTransitionAllowed(AppointmentStatus.Confirmed);  // iş kuralı içeride
        Status = AppointmentStatus.Confirmed;
        AddStatusHistory(AppointmentStatus.Confirmed, confirmedBy, notes);
    }
}
```

**Örnek — Money Value Object:**
```csharp
// Nesne oluşturulduğunda negatif değer geçilemez, sonradan da değiştirilemez
public sealed class Money
{
    public decimal Amount { get; }  // yalnızca getter
    private Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Para miktarı negatif olamaz.");
        Amount = decimal.Round(amount, 2);
    }
    public static Money Of(decimal amount, string currency = "TRY") => new(amount, currency);
}
```

---

## 2. Kalıtım (Inheritance)

**Nerede:** `Person → Patient`, `Person → Employee → Doctor`, `Person → Employee → MedicalStaff`

```
Person (soyut)
├── Patient               → MedicalRecordNumber, BloodType, UserId
└── Employee (soyut)
    ├── Doctor            → Specialization, LicenseNumber, DepartmentId
    └── MedicalStaff      → StaffRole, DepartmentId
```

**Örnek:**
```csharp
// Person soyut sınıfı ortak alanları paylaşır
public abstract class Person : BaseEntity
{
    public string FirstName { get; protected set; }
    public string FullName => $"{FirstName} {LastName}";  // kalıtsal hesaplamalı özellik
}

// Employee : Person — ek çalışan özellikleri
public abstract class Employee : Person
{
    public string EmployeeNumber { get; protected set; }
    public bool IsActive { get; protected set; }
    public abstract decimal CalculateConsultationFee();  // soyut metot
}

// Doctor : Employee — uzman doktor davranışı
public class Doctor : Employee
{
    public string Specialization { get; private set; }
    public override decimal CalculateConsultationFee()
        => Specialization.Contains("kardiyoloji") ? 800m : 500m;
}
```

---

## 3. Çok Biçimlilik (Polymorphism)

**Nerede:** `GetDisplayRole()` ve `CalculateConsultationFee()` metotları

```csharp
// Üst sınıf referansıyla farklı davranış
Person[] people = [
    new Patient(...),
    new Doctor(...),
    new MedicalStaff(...)
];

foreach (var person in people)
{
    Console.WriteLine(person.GetDisplayRole());
    // → "Hasta"
    // → "Dr. — Kardiyoloji"
    // → "Tıbbi Personel — Hemşire"
}

Employee[] employees = [ new Doctor(...), new MedicalStaff(...) ];
foreach (var emp in employees)
{
    Console.WriteLine(emp.CalculateConsultationFee());
    // Doctor  → 800.00
    // MedicalStaff → 0.00
}
```

---

## 4. Soyutlama (Abstraction)

**Nerede:** `Person`, `Employee` soyut sınıfları ve Repository interface'leri

```csharp
// Kullanıcı detayı bilmez, sadece interface'i kullanır
public interface IAppointmentService
{
    Task<AppointmentDto> CreateAppointmentAsync(Guid patientId, CreateAppointmentDto dto, CancellationToken ct = default);
}

// Repository soyutlaması — veritabanı detayı gizlenir
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
}
```

---

## 5. Bileşim (Composition)

**Nerede:** Hospital → Branch → Department → HealthcareService hiyerarşisi

```csharp
public class Hospital : BaseEntity
{
    private readonly List<Branch> _branches = [];
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();
    // Hospital, Branch nesnelerinden oluşur
}

public class Branch : BaseEntity
{
    private readonly List<Department> _departments = [];
    private readonly List<WorkingHours> _workingHours = [];
    // Branch, Department ve WorkingHours nesnelerinden oluşur
}
```

---

## 6. İlişkilendirme (Association)

**Nerede:** `Appointment → Doctor`, `Appointment → Patient`, `Doctor → DoctorAvailability`

```csharp
public class Appointment : BaseEntity
{
    // Doktor ile ilişki — aynı doktor birçok randevuyla ilişkili olabilir
    public Guid DoctorId { get; private set; }
    public Doctor? Doctor { get; private set; }  // navigasyon

    // Hasta ile ilişki
    public Guid PatientId { get; private set; }
    public Patient? Patient { get; private set; }
}
```
