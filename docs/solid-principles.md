# SOLID Prensipleri — Gerçek Kod Örnekleri

## S — Single Responsibility Principle (Tek Sorumluluk)

Her sınıfın yalnızca bir değişme sebebi vardır.

| Sınıf | Sorumluluğu |
|-------|-------------|
| `Appointment` | Randevu iş kurallarını yönetmek |
| `AppointmentService` | Randevu iş akışlarını koordine etmek |
| `AppointmentRepository` | Randevu veri erişimini sağlamak |
| `AuthService` | Kimlik doğrulama işlemlerini yürütmek |
| `NotificationService` | Bildirimleri oluşturmak ve göndermek |
| `Money` | Para birimi hesaplamalarını kapsüllemek |

```csharp
// DOĞRU: AppointmentService yalnızca iş akışını koordine eder
public class AppointmentService : IAppointmentService
{
    // Repository, bildirim servisi inject edilir — SRP korunur
    public AppointmentService(IAppointmentRepository repo, INotificationService notify) { }
}

// YANLIŞ örnek (SRP ihlali olurdu):
// public class AppointmentService { 
//     void SaveToDatabase() { } // Hem iş mantığı hem veritabanı
//     void SendEmail() { }      // Hem iş mantığı hem bildirim
// }
```

---

## O — Open/Closed Principle (Açık/Kapalı)

Genişletmeye açık, değişikliğe kapalı.

**Nerede:** `INotificationService` ve bildirim ekleme senaryosu

```csharp
// INotificationService değişmeden yeni kanal eklenebilir (SMS, Email, Push)
public interface INotificationService
{
    Task SendAppointmentCreatedAsync(string userId, Guid appointmentId, ...);
}

// Mevcut implementasyon değişmeden yeni biri eklenebilir:
// public class SmsNotificationService : INotificationService { ... }
// public class PushNotificationService : INotificationService { ... }
```

**Nerede:** Repository pattern — yeni entity için mevcut kod değişmez:
```csharp
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    // Yeni entity için: class AppointmentRepository : Repository<Appointment>
    // Repository<T> değişmez, sadece genişletilir
}
```

---

## L — Liskov Substitution Principle (Liskov Yerine Geçme)

Alt sınıflar üst sınıfın yerine kullanılabilir.

```csharp
// Person listesine Doctor ve Patient eklenebilir
IReadOnlyList<Person> people = GetAllPersons(); // Doctor ve Patient içerir
foreach (var p in people)
{
    Console.WriteLine(p.FullName); // Her ikisi de çalışır
    Console.WriteLine(p.Age);      // Her ikisi de çalışır
}

// Employee yerine Doctor veya MedicalStaff kullanılabilir
Employee emp = new Doctor(...);
emp.Deactivate();         // Doctor için çalışır
emp.TransferToBranch(id); // Doctor için çalışır

Employee emp2 = new MedicalStaff(...);
emp2.Deactivate();         // MedicalStaff için de çalışır
```

---

## I — Interface Segregation Principle (Arayüz Ayrımı)

İstemciler kullanmadıkları metotlara bağımlı olmamalıdır.

```csharp
// DOĞRU: Küçük, amaca yönelik interface'ler
public interface IAppointmentService     // Yalnızca randevu işlemleri
{
    Task<AppointmentDto> CreateAppointmentAsync(...);
    Task ConfirmAppointmentAsync(...);
}

public interface IDoctorService          // Yalnızca doktor işlemleri
{
    Task<DoctorDto> GetDoctorAsync(Guid id, ...);
    Task<IReadOnlyList<DoctorAvailabilityDto>> GetDoctorAvailabilityAsync(...);
}

public interface INotificationService    // Yalnızca bildirim işlemleri
{
    Task SendAppointmentCreatedAsync(...);
}

// YANLIŞ örnek (ISP ihlali olurdu):
// public interface IMegaService {
//     Task CreateAppointment();
//     Task GetDoctor();
//     Task SendNotification();
//     Task CreateHospital();
// }
```

---

## D — Dependency Inversion Principle (Bağımlılıkları Tersine Çevirme)

Üst seviye modüller implementasyonlara değil soyutlamalara bağımlı olmalıdır.

```csharp
// AppointmentService soyutlamalara bağlıdır
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _repo;    // interface
    private readonly IDoctorRepository _doctorRepo;   // interface
    private readonly IUnitOfWork _unitOfWork;         // interface
    private readonly INotificationService _notify;    // interface

    public AppointmentService(
        IAppointmentRepository repo,
        IDoctorRepository doctorRepo,
        IUnitOfWork unitOfWork,
        INotificationService notify)
    {
        // Somut sınıflara bağımlılık yok
    }
}

// Program.cs'de bağımlılıklar inject edilir
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
```
