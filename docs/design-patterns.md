# Tasarım Desenleri

## Kullanılan Desenler

### 1. Repository Pattern
**Dosyalar:** `IRepository<T>`, `Repository<T>`, `AppointmentRepository`, `DoctorRepository` vb.

Veri erişim mantığını iş mantığından ayırır. Test edilebilirlik sağlar.

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
}
// Kullanım: servis katmanı veritabanını bilmez
```

### 2. Unit of Work Pattern
**Dosyalar:** `IUnitOfWork`, `UnitOfWork`

Birden fazla repository değişikliğini tek transaction'da toplar.

```csharp
await _unitOfWork.BeginTransactionAsync();
await _appointmentRepo.AddAsync(appointment);
await _notificationRepo.AddAsync(notification);
await _unitOfWork.CommitTransactionAsync(); // ya hepsi ya hiçbiri
```

### 3. Factory Method (Static)
**Dosyalar:** `DoctorAvailability`, `Money`, `Address`

Nesne oluşturmayı kapsülleyerek geçersiz durum engellenir.

```csharp
// Haftalık program veya özel tarih — aynı arayüz, farklı davranış
var weekly = DoctorAvailability.CreateWeeklySchedule(doctorId, DayOfWeek.Monday, ...);
var leave  = DoctorAvailability.CreateLeaveDay(doctorId, date, "Yıllık izin");
var money  = Money.Of(500m, "TRY");
```

### 4. Value Object Pattern
**Dosyalar:** `Money`, `Address`

Kimliği olmayan, değeriyle tanımlanan nesneler. Immutable.

```csharp
var price1 = Money.Of(500m, "TRY");
var price2 = Money.Of(500m, "TRY");
price1 == price2; // true — referans değil değer karşılaştırması
```

### 5. Strategy Pattern (implicit)
**Dosyalar:** `INotificationService`, `Doctor.CalculateConsultationFee()`

Farklı bildirim stratejileri veya ücret hesaplama stratejileri değiştirilebilir.

### 6. Decorator (Middleware)
**Dosyalar:** `ExceptionHandlingMiddleware`

API'ye gelen her isteği sarar, hataları merkezi yönetir.

### 7. Extension Method Pattern
**Dosyalar:** `ApplicationServiceExtensions`, `InfrastructureServiceExtensions`

DI kayıt kodunu modüler ve okunabilir tutar.

```csharp
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(configuration);
```
