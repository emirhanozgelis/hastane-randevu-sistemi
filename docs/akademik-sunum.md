# Hastane ve Klinik Randevu Yönetim Sistemi
## Akademik Sunum Belgesi — OOP / SOLID / Clean Architecture

**Dil:** C# 14 | **Framework:** .NET 10 LTS | **Veritabanı:** PostgreSQL 17  
**Platform:** macOS (Apple Silicon) | **Mimari:** Clean Architecture

---

## 1. Proje Mimarisi

```
HospitalProject/
├── src/
│   ├── HospitalAppointment.Domain/          ← İş kuralları, dış bağımlılık YOK
│   ├── HospitalAppointment.Application/     ← İş akışları, servis arayüzleri, DTO'lar
│   ├── HospitalAppointment.Infrastructure/  ← EF Core, PostgreSQL, Identity, JWT
│   ├── HospitalAppointment.API/             ← REST API, Swagger, JWT auth
│   └── HospitalAppointment.Web/             ← MVC, Razor Views, Bootstrap 5
└── tests/
    ├── HospitalAppointment.Domain.Tests/    ← 36 unit test
    ├── HospitalAppointment.Application.Tests/ ← 3 unit test (Moq)
    └── HospitalAppointment.IntegrationTests/  ← 7 PostgreSQL integration test
```

**Bağımlılık Yönü:** Web/API → Application → Domain (← Infrastructure)
**Domain katmanı hiçbir dış pakete bağımlı değildir.**

---

## 2. OOP Prensipleri — Gerçek Kod Örnekleri

### 2.1 Kapsülleme (Encapsulation)

```csharp
// Appointment.cs — durum değiştirme sadece kendi metotları üzerinden
public class Appointment : BaseEntity
{
    public AppointmentStatus Status { get; private set; }  // ← private setter

    public void Confirm(string confirmedBy, string? notes = null)
    {
        EnsureStatusTransitionAllowed(AppointmentStatus.Confirmed); // iş kuralı içeride
        Status = AppointmentStatus.Confirmed;
        AddStatusHistory(AppointmentStatus.Confirmed, confirmedBy, notes);
    }
}

// Money.cs — negatif değer geçilemez
public sealed class Money
{
    public decimal Amount { get; }  // yalnızca getter
    private Money(decimal amount, string currency)
    {
        if (amount < 0) throw new ArgumentException("Para miktarı negatif olamaz.");
    }
    public static Money Of(decimal amount, string currency = "TRY") => new(amount, currency);
}
```

### 2.2 Kalıtım (Inheritance)

```
Person (soyut)
├── Patient               — MedicalRecordNumber, BloodType, UserId
└── Employee (soyut)
    ├── Doctor            — Specialization, LicenseNumber, DepartmentId
    └── MedicalStaff      — StaffRole, DepartmentId
```

```csharp
// Person → Employee → Doctor üç seviyeli kalıtım
public abstract class Person : BaseEntity
{
    public string FirstName { get; protected set; }
    public string FullName => $"{FirstName} {LastName}";  // kalıtsal hesaplamalı özellik
    public abstract string GetDisplayRole();              // soyut metot
}

public abstract class Employee : Person
{
    public string EmployeeNumber { get; protected set; }
    public abstract decimal CalculateConsultationFee();  // soyut metot
}

public class Doctor : Employee
{
    public string Specialization { get; private set; }
    public override string GetDisplayRole() => $"Dr. — {Specialization}";
    public override decimal CalculateConsultationFee()
        => Specialization.Contains("Kardiyoloji") ? 800m : 500m;
}
```

### 2.3 Çok Biçimlilik (Polymorphism)

```csharp
// Üst sınıf referansıyla farklı davranış
Employee[] employees = [ new Doctor(...), new MedicalStaff(...) ];

foreach (var emp in employees)
{
    Console.WriteLine(emp.GetDisplayRole());
    // Doctor      → "Dr. — Kardiyoloji"
    // MedicalStaff → "Tıbbi Personel — Hemşire"

    Console.WriteLine(emp.CalculateConsultationFee());
    // Doctor       → 800.00 (Kardiyoloji)
    // MedicalStaff → 0.00
}
```

### 2.4 Soyutlama (Abstraction)

```csharp
// Repository arayüzü — veritabanı detayı gizlenir
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
}

// Servis arayüzü — iş akışı detayı gizlenir
public interface IAppointmentService
{
    Task<AppointmentDto> CreateAppointmentAsync(Guid patientId, CreateAppointmentDto dto, ...);
    Task ConfirmAppointmentAsync(Guid appointmentId, string confirmedBy, ...);
}
```

### 2.5 Bileşim (Composition)

```csharp
// Hospital → Branch → Department → HealthcareService hiyerarşisi
public class Hospital : BaseEntity
{
    private readonly List<Branch> _branches = [];
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();
}

public class Branch : BaseEntity
{
    private readonly List<Department> _departments = [];
    private readonly List<WorkingHours> _workingHours = [];
}
```

### 2.6 İlişkilendirme (Association)

```csharp
// Appointment birden fazla entity ile ilişkili
public class Appointment : BaseEntity
{
    public Guid DoctorId { get; private set; }   // Doctor ile association
    public Guid PatientId { get; private set; }  // Patient ile association
    public Doctor? Doctor { get; private set; }  // navigasyon
    public Patient? Patient { get; private set; }
}
```

---

## 3. SOLID Prensipleri — Gerçek Kod Örnekleri

### 3.1 SRP — Tek Sorumluluk

| Sınıf | Sorumluluğu |
|-------|-------------|
| `Appointment` | Randevu iş kurallarını yönetmek |
| `AppointmentService` | Randevu iş akışını koordine etmek |
| `AppointmentRepository` | Randevu veri erişimini sağlamak |
| `AuthService` | JWT kimlik doğrulamayı yönetmek |
| `NotificationService` | Bildirimleri oluşturmak/göndermek |
| `Money` | Para birimi hesaplamalarını kapsüllemek |

### 3.2 OCP — Açık/Kapalı

```csharp
// INotificationService değişmeden yeni kanal eklenebilir
public interface INotificationService
{
    Task SendAppointmentCreatedAsync(string userId, Guid appointmentId, ...);
}
// Mevcut kod değişmeden:
// class SmsNotificationService : INotificationService { ... }
// class PushNotificationService : INotificationService { ... }

// Repository<T> değişmeden yeni entity için genişletilebilir
public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository { }
```

### 3.3 LSP — Liskov Yerine Geçme

```csharp
// Person listesi Doctor ve Patient içerebilir
IReadOnlyList<Person> people = GetAllPersons();
foreach (var p in people)
{
    p.FullName.ToString();  // Her ikisi için de çalışır
    p.GetDisplayRole();     // Polimorfik — farklı sonuç verir
}
```

### 3.4 ISP — Arayüz Ayrımı

```csharp
// Küçük, amaca yönelik arayüzler — tek büyük "God Interface" yok
public interface IAppointmentService     { ... }  // sadece randevu
public interface IDoctorService          { ... }  // sadece doktor
public interface INotificationService    { ... }  // sadece bildirim
public interface IHealthcareServiceService { ... } // sadece hizmet
```

### 3.5 DIP — Bağımlılık Tersine Çevirme

```csharp
// AppointmentService soyutlamalara bağlıdır, implementasyonlara değil
public sealed class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repo;    // interface
    private readonly IDoctorRepository _doctorRepo;   // interface
    private readonly IUnitOfWork _unitOfWork;         // interface
    private readonly INotificationService _notify;    // interface
    // → Hiçbir somut sınıf referansı yok
}

// DI kaydı (Program.cs)
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
```

---

## 4. Kullanılan Tasarım Desenleri

| Desen | Dosyalar | Amaç |
|-------|---------|------|
| **Repository Pattern** | `IRepository<T>`, `Repository<T>` | Veri erişimini soyutlar |
| **Unit of Work** | `IUnitOfWork`, `UnitOfWork` | Transaction yönetimi |
| **Factory Method** | `Money.Of()`, `DoctorAvailability.CreateWeeklySchedule()` | Nesne oluşturmayı kapsüller |
| **Value Object** | `Money`, `Address` | Kimliksiz, değer bazlı nesneler |
| **Strategy** | `CalculateConsultationFee()` | Farklı hesaplama stratejileri |
| **Decorator (Middleware)** | `ExceptionHandlingMiddleware` | Cross-cutting concern |
| **Extension Method** | `ApplicationServiceExtensions` | Modüler DI kaydı |

---

## 5. Veritabanı İlişkileri

```
Hospitals (1) ──── (*) Branches (1) ──── (*) Departments (1) ──── (*) HealthcareServices
              │                (1) ──── (*) WorkingHours
              └──── Branches (1) ──── (*) Doctors (1) ──── (*) DoctorAvailabilities
                                             (1) ──── (*) Appointments ──── (*) AppointmentStatusHistories
                                      Patients (1) ──── (*) Appointments
```

**21 tablo** — Identity (9) + Domain (12)

**Özel özellikler:**
- `Money` value object: `OwnsOne` ile 2 sütuna dönüştürülür
- `TimeSpan`: `long` (ticks) olarak kaydedilir
- `DateOnly`/`TimeOnly`: Npgsql 10 native desteği
- Randevu snapshot: fiyat ve süre oluşturulurken kopyalanır

---

## 6. Randevu İş Akışı

```
Hasta → Hastane/Şube/Bölüm/Doktor seçer
      → Tarih ve saat seçer
      → CreateAppointmentAsync()
            ↓
        [Advisory Lock — aynı doktor için serileştirilir]
            ↓
        Doktor müsaitlik kontrolü
        Çakışma kontrolü (doktor)
        Çakışma kontrolü (hasta)
            ↓
        Appointment oluşturulur (Status: Pending)
        Bildirim gönderilir
            ↓
Doktor/Admin → Confirm() → Status: Confirmed
            veya Reject(reason) → Status: Rejected

Hasta/Doktor → Cancel() → Status: Cancelled
Doktor/Admin → Complete() → Status: Completed
Doktor/Admin → MarkAsNoShow() → Status: NoShow
```

**Concurrency koruması:**  
`pg_advisory_xact_lock` ile aynı doktor için eş zamanlı iki randevu isteği PostgreSQL tarafından serileştirilir.

---

## 7. Olası Sorular ve Cevaplar

**S1: Neden Clean Architecture seçtiniz?**  
C: Domain katmanı dış teknolojilere bağımlı olmaz. Veritabanı veya framework değiştirilirse yalnızca Infrastructure katmanı etkilenir. Test edilebilirlik yüksektir.

**S2: `Person` soyut sınıfı neden doğrudan DbSet olarak tanımlanmadı?**  
C: `Person` soyut olduğu için EF Core onu doğrudan tablo olarak mapleyemez. TPC (Table Per Concrete type) stratejisiyle `Doctor`, `Patient`, `MedicalStaff` her biri kendi tablosuna sahiptir.

**S3: `Money` neden `struct` değil `class`?**  
C: EF Core'un `OwnsOne` ile Value Object maplemesi `class` gerektirir. Ayrıca `sealed class` immutability sağlar.

**S4: Randevu çakışması nasıl tespit edilir?**  
C: `NewStart < ExistingEnd AND NewEnd > ExistingStart` koşuluyla. İptal ve reddedilen randevular çakışma sayılmaz.

**S5: PostgreSQL advisory lock neden kullandınız?**  
C: Aynı anda iki kullanıcı aynı doktora aynı saate randevu almaya çalışırsa, uygulama seviyesi `SELECT + INSERT` arasında race condition oluşabilir. `pg_advisory_xact_lock` bu isteği serileştirerek sadece birinin başarılı olmasını sağlar.

**S6: JWT ve Cookie auth aynı anda neden kullanılıyor?**  
C: API (REST) stateless olduğundan JWT tercih edilir — mobil ve SPA uyumlu. Web (MVC) browser tabanlı olduğundan Cookie auth kullanılır — session yönetimi için doğal.

**S7: `IPatientService` neden yok, `IPatientRepository` doğrudan inject ediliyor?**  
C: Patient için özel bir iş kuralı servisi gerekmedi — CRUD işlemleri yeterli. Gereksiz abstraction eklemekten kaçınıldı (YAGNI).

**S8: `AppointmentDto`'da neden snapshot fiyat var?**  
C: Hizmet fiyatı sonradan değiştirilirse geçmiş randevuların ücretleri etkilenmemelidir. Randevu oluşturulurken fiyat anlık olarak kopyalanır.

**S9: `CalculateConsultationFee()` polimorfik nasıl kullanılır?**  
C: `Employee[] list` referansıyla hem `Doctor` hem `MedicalStaff` nesneleri tutulabilir. `list[i].CalculateConsultationFee()` çağrısı runtime'da doğru sınıfın metodunu çağırır.

**S10: Randevu durum geçişi iş kuralı nerede tutulur?**  
C: `Appointment.EnsureStatusTransitionAllowed()` metodunda — domain entity içinde. Controller veya service'te değil. Bu kapsüllemenin önemli bir örneği.

**S11: Neden `private setter` kullanıldı?**  
C: Encapsulation prensibi gereği dışarıdan doğrudan atama engellenir. Durum değişikliği ancak entity'nin kendi iş kuralı metodları üzerinden yapılabilir.

**S12: `DoctorAvailability.CreateWeeklySchedule()` neden static factory?**  
C: Farklı oluşturma senaryolarını (haftalık, özel tarih, izin günü) ayırt etmek için. Constructor overloading yerine daha okunabilir ve niyeti açık bir API sunar.

**S13: `IUnitOfWork` neden ayrı bir arayüz?**  
C: Transaction yönetimini repository'den ayırır. Birden fazla repository değişikliğini tek bir transaction'da toplamak için servis katmanı `IUnitOfWork.BeginTransactionAsync()` çağırır.

**S14: Test piramidi nasıl kuruldu?**  
C: 36 domain unit testi (hızlı, dış bağımlılık yok), 3 application unit testi (Moq ile mock), 7 integration testi (gerçek PostgreSQL). Toplam 46 test, hepsi yeşil.

**S15: `AsNoTracking()` ne zaman kullanıldı?**  
C: Salt okunur sorgularda (listeleme işlemleri). `AsNoTracking` ile EF Core change tracking overhead'i kaldırılır, performans artar.

**S16: `AsSplitQuery()` ne zaman kullanıldı?**  
C: Birden fazla collection include edildiğinde Cartesian explosion'ı önlemek için. Örn: `Include(d => d.Availabilities).Include(d => d.Appointments)`.

**S17: Bootstrap 5 neden seçildi?**  
C: macOS'ta herhangi bir tool gerektirmeden CDN üzerinden kullanılabilir. .NET projesine build adımı eklemez. Responsive, modern ve Türkçe içerikle uyumlu.

**S18: Seed data nasıl yönetildi?**  
C: `DataSeeder.SeedAsync()` uygulama başlangıcında çalışır. Idempotent'tir — mevcut veri varsa atlar. `AspNetRoles`, `AspNetUsers`, `Hospitals`, `Doctors`, `Patients` oluşturur.

**S19: `FluentValidation` neden eklendi ama tam kullanılmadı?**  
C: Domain entity constructor'larında temel validasyon var. FluentValidation ileride DTO seviyesinde daha zengin validasyon için hazır bırakıldı. Gereksiz abstraction eklemeden yerinde tutuldu.

**S20: Proje hangi kısımlarında eksik?**  
C: PostgreSQL exclusion constraint (`tstzrange`) ile ek DB düzeyinde çakışma önlemi eklenebilir. SMS/e-posta bildirim entegrasyonu (şu an yalnızca DB notification). Appointment arama/filtreleme API'sı genişletilebilir. Doktor takvimi görsel görünümü (FullCalendar.js).
