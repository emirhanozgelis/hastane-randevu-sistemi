# Mimari Tasarım Belgesi

## Clean Architecture

```
┌──────────────────────────────────────────────────────────────┐
│  Sunum Katmanı (Presentation)                                │
│  ┌─────────────────┐  ┌─────────────────────────────────┐   │
│  │  API (REST)      │  │  Web (MVC / Razor)              │   │
│  │  Swagger/OpenAPI │  │  Bootstrap 5 / Turkish UI       │   │
│  └────────┬─────────┘  └────────────┬────────────────────┘   │
└───────────┼──────────────────────────┼────────────────────────┘
            │                          │
┌───────────▼──────────────────────────▼────────────────────────┐
│  Uygulama Katmanı (Application)                               │
│  ┌────────────┐  ┌────────────┐  ┌──────────────────────┐    │
│  │  Services  │  │    DTOs    │  │  Service Interfaces   │    │
│  │  (Business │  │  (Request/ │  │  IAppointmentService  │    │
│  │  Workflow) │  │  Response) │  │  IDoctorService       │    │
│  └────────────┘  └────────────┘  └──────────────────────┘    │
└───────────────────────────┬────────────────────────────────────┘
                            │
┌───────────────────────────▼────────────────────────────────────┐
│  Domain Katmanı (Entities & Business Rules)                    │
│  ┌──────────┐  ┌──────────────┐  ┌────────────────────────┐  │
│  │ Entities │  │ Value Objects│  │  Domain Interfaces     │  │
│  │ Person   │  │ Money        │  │  IRepository<T>        │  │
│  │ Doctor   │  │ Address      │  │  IUnitOfWork           │  │
│  │ Patient  │  └──────────────┘  │  IAppointmentRepository│  │
│  │ Hospital │                    └────────────────────────┘  │
│  └──────────┘                                                  │
└────────────────────────────────────────────────────────────────┘
            ▲
┌───────────┴────────────────────────────────────────────────────┐
│  Altyapı Katmanı (Infrastructure)                              │
│  ┌────────────────┐  ┌──────────────┐  ┌────────────────────┐ │
│  │  EF Core 10    │  │  Repositories│  │  Identity + JWT    │ │
│  │  AppDbContext  │  │  (IRepo impls│  │  AuthService       │ │
│  │  PostgreSQL    │  │   UnitOfWork)│  │  ApplicationUser   │ │
│  │  Migrations    │  └──────────────┘  └────────────────────┘ │
│  │  Seed Data     │                                            │
│  └────────────────┘                                            │
└────────────────────────────────────────────────────────────────┘
```

## Katman Bağımlılıkları

```
Domain        → (hiçbir dış bağımlılık yok)
Application   → Domain
Infrastructure → Domain + Application
API           → Application + Infrastructure
Web           → Application + Infrastructure
```

## Varlık İlişkileri

```
Hospital (1) ──── (*) Branch (1) ──── (*) Department (1) ──── (*) HealthcareService
                      │
                      └──── (*) WorkingHours

Branch (1) ──── (*) Doctor ──── (*) DoctorAvailability
                     │
                     └──── (*) Appointment ──── (*) AppointmentStatusHistory

Appointment (*) ──── (1) Patient
Appointment (*) ──── (1) HealthcareService
```

## Teknoloji Kararları

| Karar | Neden |
|-------|-------|
| PostgreSQL | ACID garantisi, concurrent randevu için FOR UPDATE lock desteği |
| EF Core Code First | Şema değişikliklerini migration olarak takip edebilmek |
| JWT Bearer (API) | Stateless, mobil uygulama uyumlu |
| Cookie Auth (Web) | Browser session yönetimi için doğal |
| Money Value Object | Negatif ücret ve para birimi hatalarını önlemek |
| Repository Pattern | Test edilebilirlik, veritabanı bağımsızlığı |
| Snapshot fiyat | Hizmet fiyatı değiştiğinde geçmiş randevular etkilenmez |
