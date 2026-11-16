# Veritabanı Tasarımı

## Tablolar

### Hospitals
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| Name | varchar(200) | Hastane adı |
| Address_Street | varchar | Cadde |
| Address_District | varchar | İlçe |
| Address_City | varchar | Şehir |
| Address_PostalCode | varchar | Posta kodu |
| Address_Country | varchar | Ülke |
| TaxNumber | varchar | Vergi no |
| IsActive | bool | Aktif mi |
| CreatedAt | timestamptz | Oluşturma zamanı |

### Branches
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| HospitalId | uuid | FK → Hospitals.Id |
| Name | varchar(200) | Şube adı |
| PhoneNumber | varchar(20) | Telefon |
| IsActive | bool | Aktif mi |

### Departments
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| BranchId | uuid | FK → Branches.Id |
| Name | varchar(150) | Bölüm adı |
| Description | text | Açıklama |

### HealthcareServices
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| DepartmentId | uuid | FK → Departments.Id |
| Name | varchar(200) | Hizmet adı |
| Duration | bigint | Süre (ticks) |
| Price_Amount | numeric(18,2) | Ücret |
| Price_Currency | varchar(3) | Para birimi |
| IsActive | bool | Aktif mi |

### Doctors
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| FirstName | varchar | Ad |
| LastName | varchar | Soyad |
| Email | varchar | E-posta |
| Specialization | varchar(150) | Uzmanlık |
| LicenseNumber | varchar | Lisans no |
| BranchId | uuid | FK → Branches.Id |
| DepartmentId | uuid | FK → Departments.Id |
| UserId | varchar | FK → AspNetUsers.Id |
| IsActive | bool | Aktif mi |

### Patients
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| FirstName/LastName | varchar | Ad/Soyad |
| BloodType | int | Kan grubu (enum) |
| MedicalRecordNumber | varchar | Hasta kayıt no |
| UserId | varchar | FK → AspNetUsers.Id |

### Appointments
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| HospitalId | uuid | FK → Hospitals.Id |
| BranchId | uuid | FK → Branches.Id |
| DepartmentId | uuid | FK → Departments.Id |
| DoctorId | uuid | FK → Doctors.Id |
| PatientId | uuid | FK → Patients.Id |
| HealthcareServiceId | uuid | FK → HealthcareServices.Id |
| AppointmentDate | date | Tarih |
| StartTime | time | Başlangıç saati |
| EndTime | time | Bitiş saati |
| SnapshotPrice_Amount | numeric(18,2) | Anlık fiyat |
| SnapshotDuration | bigint | Anlık süre (ticks) |
| SnapshotServiceName | varchar | Anlık hizmet adı |
| Status | int | Durum (enum) |
| CreatedAt | timestamptz | Oluşturma zamanı |

### DoctorAvailabilities
| Kolon | Tip | Açıklama |
|-------|-----|----------|
| Id | uuid | PK |
| DoctorId | uuid | FK → Doctors.Id |
| Date | date | Özel tarih (MinValue=haftalık) |
| DayOfWeek | int | Haftanın günü |
| StartTime | time | Başlangıç |
| EndTime | time | Bitiş |
| IsAvailable | bool | Müsait mi |

## Concurrency Stratejisi

Aynı anda iki kullanıcı aynı doktora aynı saatte randevu almaya çalışırsa:

```sql
-- AppointmentRepository.HasDoctorConflictAsync
-- PostgreSQL optimistic concurrency + LINQ çakışma kontrolü:
-- NewStart < ExistingEnd AND NewEnd > ExistingStart
-- AND Status NOT IN (Cancelled, Rejected)
-- AND ExcludeId IS NULL OR Id != ExcludeId
```

Uygulama seviyesinde `BeginTransactionAsync` ile transaction açılarak
çakışma kontrolü ve kayıt atomik olarak yapılır.
