using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.ValueObjects;
using HospitalAppointment.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Infrastructure.Data.Seeds;

// Başlangıç verisi tohumlayıcı — geliştirme ve demo ortamları için
public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        try
        {
            // Bekleyen migration varsa uygula
            await context.Database.MigrateAsync();

            await CityDistrictSeeder.SeedAsync(context, logger);
            await SeedRolesAsync(roleManager, logger);
            await SeedHospitalHierarchyAsync(context, logger);
            await SeedUsersAndStaffAsync(context, userManager, logger);

            logger.LogInformation("Veri tohumlama başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veri tohumlama sırasında hata oluştu.");
            throw;
        }
    }

    // Rol tohumlama — 5 sistem rolü
    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole> roleManager,
        ILogger logger)
    {
        var roles = new[] { "Admin", "HospitalManager", "Doctor", "MedicalStaff", "Patient" };

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Rol oluşturuldu: {Role}", roleName);
            }
        }
    }

    // Hastane hiyerarşisi — Hastane > Şube > Bölüm > Hizmet > Çalışma Saatleri
    private static async Task SeedHospitalHierarchyAsync(
        AppDbContext context,
        ILogger logger)
    {
        // Zaten veri varsa atla
        if (await context.Hospitals.AnyAsync())
        {
            logger.LogInformation("Hastane verisi zaten mevcut, atlanıyor.");
            return;
        }

        // --- Hastane ---
        var hospital = new Hospital(
            "Merkezi Hastane",
            Address.Of("Bağdat Caddesi No:1", "Kadıköy", "İstanbul", "34710"),
            taxNumber: "1234567890",
            contactEmail: "info@merkezihastane.com");

        await context.Hospitals.AddAsync(hospital);
        await context.SaveChangesAsync();

        logger.LogInformation("Hastane oluşturuldu: {Hospital}", hospital.Name);

        // --- Şubeler ---
        var branchMerkez = new Branch(
            hospital.Id,
            "Merkez Şube",
            Address.Of("Bağdat Caddesi No:1", "Kadıköy", "İstanbul", "34710"),
            "+90 216 000 0001",
            "merkez@merkezihastane.com");

        var branchAnadolu = new Branch(
            hospital.Id,
            "Anadolu Şubesi",
            Address.Of("Üsküdar Meydanı No:5", "Üsküdar", "İstanbul", "34660"),
            "+90 216 000 0002",
            "anadolu@merkezihastane.com");

        await context.Branches.AddRangeAsync(branchMerkez, branchAnadolu);
        await context.SaveChangesAsync();

        // --- Çalışma Saatleri (her iki şube için) ---
        await SeedWorkingHoursAsync(context, branchMerkez.Id);
        await SeedWorkingHoursAsync(context, branchAnadolu.Id);

        // --- Bölümler ve Hizmetler ---
        await SeedDepartmentsAndServicesAsync(context, branchMerkez.Id, branchAnadolu.Id, logger);

        logger.LogInformation("Hastane hiyerarşisi başarıyla oluşturuldu.");
    }

    // Çalışma saatleri — Pazartesi-Cumartesi 08:00-18:00, Pazar kapalı
    private static async Task SeedWorkingHoursAsync(AppDbContext context, Guid branchId)
    {
        var openTime = new TimeOnly(8, 0);
        var closeTime = new TimeOnly(18, 0);

        var workingHoursList = new List<WorkingHours>();

        for (int day = 0; day <= 6; day++)
        {
            var dayOfWeek = (DayOfWeek)day;
            bool isClosed = dayOfWeek == DayOfWeek.Sunday;

            var wh = isClosed
                ? new WorkingHours(branchId, dayOfWeek, openTime, closeTime, isClosed: true)
                : new WorkingHours(branchId, dayOfWeek, openTime, closeTime);

            workingHoursList.Add(wh);
        }

        await context.WorkingHours.AddRangeAsync(workingHoursList);
        await context.SaveChangesAsync();
    }

    // Bölümler ve sağlık hizmetleri tohumlama
    private static async Task SeedDepartmentsAndServicesAsync(
        AppDbContext context,
        Guid branchMerkezId,
        Guid branchAnadoluId,
        ILogger logger)
    {
        var departmentDefinitions = new[]
        {
            ("Kardiyoloji", "Kalp ve damar hastalıkları bölümü"),
            ("Nöroloji", "Sinir sistemi hastalıkları bölümü"),
            ("Ortopedi", "Kas ve iskelet sistemi bölümü"),
            ("Genel Cerrahi", "Genel cerrahi operasyonlar bölümü")
        };

        var allDepartments = new List<Department>();

        // Her iki şube için aynı bölümleri oluştur
        foreach (var branchId in new[] { branchMerkezId, branchAnadoluId })
        {
            foreach (var (name, description) in departmentDefinitions)
            {
                var dept = new Department(branchId, name, description);
                await context.Departments.AddAsync(dept);
                allDepartments.Add(dept);
            }
        }

        await context.SaveChangesAsync();

        // Her bölüm için en az 2 sağlık hizmeti ekle
        foreach (var dept in allDepartments)
        {
            var services = GetServicesForDepartment(dept.Id, dept.Name);
            await context.HealthcareServices.AddRangeAsync(services);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("{Count} bölüm ve ilgili hizmetler oluşturuldu.", allDepartments.Count);
    }

    // Bölüme göre sağlık hizmetleri
    private static List<HealthcareService> GetServicesForDepartment(Guid departmentId, string departmentName)
    {
        return departmentName switch
        {
            "Kardiyoloji" =>
            [
                new HealthcareService(departmentId, "Kardiyoloji Muayenesi", TimeSpan.FromMinutes(30),
                    Money.Of(800, "TRY"), "Kalp ve damar sistemi genel muayenesi"),
                new HealthcareService(departmentId, "EKG Çekimi", TimeSpan.FromMinutes(20),
                    Money.Of(400, "TRY"), "Elektrokardiyogram çekimi ve değerlendirme"),
                new HealthcareService(departmentId, "Ekokardiyografi", TimeSpan.FromMinutes(45),
                    Money.Of(1200, "TRY"), "Kalp ultrasonografisi")
            ],
            "Nöroloji" =>
            [
                new HealthcareService(departmentId, "Nöroloji Muayenesi", TimeSpan.FromMinutes(30),
                    Money.Of(750, "TRY"), "Sinir sistemi genel muayenesi"),
                new HealthcareService(departmentId, "EEG Çekimi", TimeSpan.FromMinutes(60),
                    Money.Of(600, "TRY"), "Elektroensefalografi çekimi"),
                new HealthcareService(departmentId, "Baş Ağrısı Konsültasyonu", TimeSpan.FromMinutes(30),
                    Money.Of(600, "TRY"), "Migren ve baş ağrısı değerlendirmesi")
            ],
            "Ortopedi" =>
            [
                new HealthcareService(departmentId, "Ortopedi Muayenesi", TimeSpan.FromMinutes(30),
                    Money.Of(700, "TRY"), "Kas ve iskelet sistemi genel muayenesi"),
                new HealthcareService(departmentId, "Fizik Tedavi Değerlendirme", TimeSpan.FromMinutes(45),
                    Money.Of(500, "TRY"), "Fizik tedavi ve rehabilitasyon değerlendirmesi"),
                new HealthcareService(departmentId, "Spor Yaralanması Konsültasyonu", TimeSpan.FromMinutes(30),
                    Money.Of(650, "TRY"), "Spor kaynaklı yaralanma değerlendirmesi")
            ],
            "Genel Cerrahi" =>
            [
                new HealthcareService(departmentId, "Genel Cerrahi Muayenesi", TimeSpan.FromMinutes(30),
                    Money.Of(600, "TRY"), "Cerrahi değerlendirme muayenesi"),
                new HealthcareService(departmentId, "Yara Bakımı", TimeSpan.FromMinutes(20),
                    Money.Of(300, "TRY"), "Yara temizliği ve pansuman"),
                new HealthcareService(departmentId, "Preoperatif Değerlendirme", TimeSpan.FromMinutes(45),
                    Money.Of(900, "TRY"), "Ameliyat öncesi değerlendirme ve hazırlık")
            ],
            _ =>
            [
                new HealthcareService(departmentId, $"{departmentName} Muayenesi", TimeSpan.FromMinutes(30),
                    Money.Of(500, "TRY"), "Genel muayene"),
                new HealthcareService(departmentId, $"{departmentName} Konsültasyonu", TimeSpan.FromMinutes(45),
                    Money.Of(600, "TRY"), "Uzman konsültasyonu")
            ]
        };
    }

    // Kullanıcı ve personel tohumlama — Admin, 2 Doktor, 2 Hasta
    private static async Task SeedUsersAndStaffAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        // İlk şubeyi ve bölümleri al
        var branch = await context.Branches.FirstOrDefaultAsync();
        if (branch is null)
        {
            logger.LogWarning("Şube bulunamadı, kullanıcı tohumlama atlanıyor.");
            return;
        }

        var kardiyolojiDept = await context.Departments
            .FirstOrDefaultAsync(d => d.BranchId == branch.Id && d.Name == "Kardiyoloji");
        var norolojiDept = await context.Departments
            .FirstOrDefaultAsync(d => d.BranchId == branch.Id && d.Name == "Nöroloji");

        // --- Admin ---
        await CreateUserWithRoleAsync(
            userManager,
            new ApplicationUser
            {
                UserName = "admin@hospital.com",
                Email = "admin@hospital.com",
                FirstName = "Sistem",
                LastName = "Admin",
                EmailConfirmed = true
            },
            "Admin123!",
            "Admin",
            logger);

        // --- Doktorlar ---
        if (kardiyolojiDept is not null && !await context.Doctors.AnyAsync())
        {
            var doctorUser1 = new ApplicationUser
            {
                UserName = "doctor1@hospital.com",
                Email = "doctor1@hospital.com",
                FirstName = "Ahmet",
                LastName = "Yılmaz",
                EmailConfirmed = true
            };
            var user1 = await CreateUserWithRoleAsync(userManager, doctorUser1, "Doctor123!", "Doctor", logger);

            if (user1 is not null)
            {
                var doctor1 = new Doctor(
                    "Ahmet", "Yılmaz",
                    "doctor1@hospital.com", "+90 532 000 0001",
                    new DateOnly(1975, 3, 15), Gender.Male,
                    "DOC001", new DateOnly(2010, 1, 1),
                    branch.Id, kardiyolojiDept.Id,
                    "Kardiyoloji", "LIC001",
                    user1.Id);

                await context.Doctors.AddAsync(doctor1);
                await context.SaveChangesAsync();

                // Haftalık program — Pazartesi-Cuma 09:00-17:00
                await SeedDoctorAvailabilityAsync(context, doctor1.Id);
                logger.LogInformation("Doktor oluşturuldu: {Name}", doctor1.FullName);
            }

            if (norolojiDept is not null)
            {
                var doctorUser2 = new ApplicationUser
                {
                    UserName = "doctor2@hospital.com",
                    Email = "doctor2@hospital.com",
                    FirstName = "Ayşe",
                    LastName = "Kaya",
                    EmailConfirmed = true
                };
                var user2 = await CreateUserWithRoleAsync(userManager, doctorUser2, "Doctor123!", "Doctor", logger);

                if (user2 is not null)
                {
                    var doctor2 = new Doctor(
                        "Ayşe", "Kaya",
                        "doctor2@hospital.com", "+90 532 000 0002",
                        new DateOnly(1980, 7, 22), Gender.Female,
                        "DOC002", new DateOnly(2012, 6, 1),
                        branch.Id, norolojiDept.Id,
                        "Nöroloji", "LIC002",
                        user2.Id);

                    await context.Doctors.AddAsync(doctor2);
                    await context.SaveChangesAsync();

                    await SeedDoctorAvailabilityAsync(context, doctor2.Id);
                    logger.LogInformation("Doktor oluşturuldu: {Name}", doctor2.FullName);
                }
            }
        }

        // --- Hastalar ---
        if (!await context.Patients.AnyAsync())
        {
            await SeedPatientAsync(
                context, userManager,
                "patient1@hospital.com", "Patient123!",
                "Mehmet", "Demir", "+90 533 000 0001",
                new DateOnly(1990, 5, 10), Gender.Male,
                BloodType.APositive, logger);

            await SeedPatientAsync(
                context, userManager,
                "patient2@hospital.com", "Patient123!",
                "Fatma", "Şahin", "+90 533 000 0002",
                new DateOnly(1985, 11, 20), Gender.Female,
                BloodType.BNegative, logger);
        }
    }

    // Doktor haftalık müsaitlik programı — Pazartesi-Cuma 09:00-17:00
    private static async Task SeedDoctorAvailabilityAsync(AppDbContext context, Guid doctorId)
    {
        var startTime = new TimeOnly(9, 0);
        var endTime = new TimeOnly(17, 0);

        var availabilities = new List<DoctorAvailability>();

        // Pazartesi (1) - Cuma (5) arası çalışma günleri
        for (int day = 1; day <= 5; day++)
        {
            var availability = DoctorAvailability.CreateWeeklySchedule(
                doctorId,
                (DayOfWeek)day,
                startTime,
                endTime);

            availabilities.Add(availability);
        }

        await context.DoctorAvailabilities.AddRangeAsync(availabilities);
        await context.SaveChangesAsync();
    }

    // Hasta oluştur ve Identity kullanıcısıyla ilişkilendir
    private static async Task SeedPatientAsync(
        AppDbContext context,
        UserManager<ApplicationUser> userManager,
        string email, string password,
        string firstName, string lastName, string phone,
        DateOnly dateOfBirth, Gender gender, BloodType bloodType,
        ILogger logger)
    {
        var appUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            EmailConfirmed = true
        };

        var user = await CreateUserWithRoleAsync(userManager, appUser, password, "Patient", logger);
        if (user is null) return;

        var patient = new Patient(
            firstName, lastName, email, phone,
            dateOfBirth, gender,
            user.Id, bloodType);

        await context.Patients.AddAsync(patient);
        await context.SaveChangesAsync();

        logger.LogInformation("Hasta oluşturuldu: {Name}", patient.FullName);
    }

    // Kullanıcı oluştur ve rol ata — mevcut kullanıcıyı güncelle
    private static async Task<ApplicationUser?> CreateUserWithRoleAsync(
        UserManager<ApplicationUser> userManager,
        ApplicationUser user,
        string password,
        string role,
        ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(user.Email!);
        if (existing is not null)
        {
            logger.LogInformation("Kullanıcı zaten mevcut, atlanıyor: {Email}", user.Email);
            return existing;
        }

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Kullanıcı oluşturulamadı {Email}: {Errors}", user.Email, errors);
            return null;
        }

        await userManager.AddToRoleAsync(user, role);
        logger.LogInformation("Kullanıcı oluşturuldu: {Email} [{Role}]", user.Email, role);
        return user;
    }
}
