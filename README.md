# Hastane ve Klinik Randevu Yönetim Sistemi

**Dil:** C# 14 | **Framework:** .NET 10 LTS | **Veritabanı:** PostgreSQL  
**Mimari:** Clean Architecture | **UI:** ASP.NET Core MVC + Bootstrap 5 | **API:** REST + Swagger

---

## Proje Hakkında

Bilgisayar Mühendisliği yüksek lisans programı OOP dersi kapsamında geliştirilmiş, çok hastaneli/çok şubeli Randevu Yönetim Sistemi. OOP ve SOLID prensiplerini gerçek kod üzerinde uygular.

---

## macOS Kurulum Durumu (Doğrulanmış — Apple M5 Pro)

| Araç | Sürüm | Durum |
|------|-------|-------|
| .NET 10 SDK | 10.0.404 | ✓ Kurulu — `~/.dotnet/` |
| PostgreSQL | 17.12 (Homebrew) | ✓ Kurulu ve çalışıyor |
| EF Core CLI | 11.0.0 | ✓ Kurulu — `~/.dotnet/tools/` |
| Git | 2.55.0 | ✓ Kurulu |

> `~/.zshrc` içine `DOTNET_ROOT` ve PATH ayarları eklenmiştir.  
> Yeni terminal açıldığında `dotnet` komutu doğrudan çalışır.

### Kurulum Gerektiren Sistemler İçin

```bash
# .NET 10 SDK kur (sudo gerektirmez)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0 --install-dir "$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"

# PostgreSQL kur
brew install postgresql@17
brew services start postgresql@17

# EF Core CLI kur
dotnet tool install --global dotnet-ef
```

---

## Hızlı Başlangıç (Docker ile)

```bash
# Depoyu klonla
cd ~/Desktop/Projects/HospitalProject

# PostgreSQL ve uygulamaları başlat
docker compose up -d

# API: http://localhost:5001
# Web: http://localhost:5002
# Swagger: http://localhost:5001/swagger
```

---

## Yerel Kurulum (PostgreSQL yörel)

### 1. PostgreSQL Kur ve Başlat

```bash
# Homebrew ile kur
brew install postgresql@17
brew services start postgresql@17

# Veritabanı oluştur
psql postgres -c "CREATE DATABASE \"HospitalDB\";"
psql postgres -c "CREATE USER hospital_user WITH PASSWORD 'hospital_pass';"
psql postgres -c "GRANT ALL PRIVILEGES ON DATABASE \"HospitalDB\" TO hospital_user;"
```

### 2. Bağlantı Dizesini Güncelle

`src/HospitalAppointment.API/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=HospitalDB;Username=postgres;Password="
  }
}
```

### 3. NuGet Paketlerini Yükle

```bash
cd /Users/emirhanozgelis/Desktop/Projects/HospitalProject
dotnet restore
```

### 4. Veritabanı Migration

```bash
cd src/HospitalAppointment.Infrastructure
dotnet ef migrations add InitialCreate \
  --startup-project ../HospitalAppointment.API \
  --context AppDbContext

dotnet ef database update \
  --startup-project ../HospitalAppointment.API \
  --context AppDbContext
```

### 5. Projeyi Derle

```bash
cd /Users/emirhanozgelis/Desktop/Projects/HospitalProject
dotnet build
```

### 6. API'yi Çalıştır

```bash
cd src/HospitalAppointment.API
dotnet run
# → http://localhost:5001
# → http://localhost:5001/swagger (Swagger UI)
```

### 7. Web Arayüzünü Çalıştır

```bash
cd src/HospitalAppointment.Web
dotnet run
# → http://localhost:5002
```

### 8. Testleri Çalıştır

```bash
cd /Users/emirhanozgelis/Desktop/Projects/HospitalProject
dotnet test --verbosity normal
```

---

## Demo Kullanıcılar

| Rol | E-posta | Şifre |
|-----|---------|-------|
| Admin | admin@hospital.com | Admin123! |
| Doktor | doctor1@hospital.com | Doctor123! |
| Doktor | doctor2@hospital.com | Doctor123! |
| Hasta | patient1@hospital.com | Patient123! |
| Hasta | patient2@hospital.com | Patient123! |

---

## Proje Mimarisi

```
HospitalProject/
├── src/
│   ├── HospitalAppointment.Domain/          ← Entity'ler, Value Object'ler, Enum'lar, Interface'ler
│   ├── HospitalAppointment.Application/     ← Servis interface'leri, DTO'lar, Servis implementasyonları
│   ├── HospitalAppointment.Infrastructure/  ← EF Core, PostgreSQL, Repositories, Identity, JWT
│   ├── HospitalAppointment.API/             ← REST API Controller'ları, Swagger
│   └── HospitalAppointment.Web/             ← MVC Controller'ları, Razor Views, Bootstrap 5
├── tests/
│   ├── HospitalAppointment.Domain.Tests/
│   ├── HospitalAppointment.Application.Tests/
│   └── HospitalAppointment.IntegrationTests/
├── docs/
│   ├── architecture.md
│   ├── oop-principles.md
│   ├── solid-principles.md
│   └── database-design.md
├── docker-compose.yml
└── README.md
```

---

## OOP Kalıtım Hiyerarşisi

```
Person (soyut)
├── Patient              → Hasta, randevu alabilir
└── Employee (soyut)
    ├── Doctor           → Uzman doktor, randevu onaylar
    └── MedicalStaff     → Tıbbi personel, destek hizmetleri
```

---

## Adresler

| Hizmet | URL |
|--------|-----|
| REST API | http://localhost:5001 |
| Swagger UI | http://localhost:5001/swagger |
| Web Arayüzü | http://localhost:5002 |
| PostgreSQL | localhost:5432 |

---

## Teknoloji Yığını

- **C# 14 / .NET 10 LTS** — macOS (Apple Silicon + Intel) uyumlu
- **ASP.NET Core 10** — Kestrel web server
- **Entity Framework Core 10 + Npgsql** — Code First, Fluent API
- **PostgreSQL 17** — Concurrency-safe transaction lock'ları
- **ASP.NET Core Identity** — Rol tabanlı yetkilendirme
- **JWT Bearer** — API kimlik doğrulama
- **Bootstrap 5.3** — Responsive UI
- **Swagger / OpenAPI** — API dokümantasyonu
- **xUnit + Moq + FluentAssertions** — Unit testler

---

## Uygulamayı Başlatma ve Durdurma

### Ön Koşul — PATH Ayarı

```bash
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
```

> `~/.zshrc` dosyasına eklendi — yeni terminalden otomatik geçerlidir.

### PostgreSQL Başlat

```bash
brew services start postgresql@17
# Durum kontrolü:
brew services list | grep postgresql
```

### API Başlat (Port 5001)

```bash
cd /Users/emirhanozgelis/Desktop/Projects/HospitalProject
dotnet run --project src/HospitalAppointment.API

# Arka planda başlat:
dotnet run --project src/HospitalAppointment.API &
```

### Web Arayüzü Başlat (Port 5002)

```bash
cd /Users/emirhanozgelis/Desktop/Projects/HospitalProject
dotnet run --project src/HospitalAppointment.Web

# Arka planda başlat:
dotnet run --project src/HospitalAppointment.Web &
```

### Durdurma

```bash
# Çalışan dotnet process'leri durdur
pkill -f "HospitalAppointment.API"
pkill -f "HospitalAppointment.Web"

# PostgreSQL durdur (gerekirse)
brew services stop postgresql@17
```

### Testleri Çalıştır

```bash
cd /Users/emirhanozgelis/Desktop/Projects/HospitalProject

# Unit testler (PostgreSQL gerekmez)
dotnet test tests/HospitalAppointment.Domain.Tests
dotnet test tests/HospitalAppointment.Application.Tests

# Integration testler (PostgreSQL gerekli)
psql -d postgres -c 'CREATE DATABASE "HospitalDB_Test";' 2>/dev/null || true
TEST_CONNECTION_STRING="Host=localhost;Port=5432;Database=HospitalDB_Test;Username=postgres;Password=postgres;Include Error Detail=true" \
  dotnet test tests/HospitalAppointment.IntegrationTests

# Tüm testler
TEST_CONNECTION_STRING="Host=localhost;Port=5432;Database=HospitalDB_Test;Username=postgres;Password=postgres;Include Error Detail=true" \
  dotnet test
```

### Erişim Adresleri

| Servis | URL |
|--------|-----|
| REST API | http://localhost:5001 |
| Swagger UI | http://localhost:5001/swagger/index.html |
| Web Arayüzü | http://localhost:5002 |
