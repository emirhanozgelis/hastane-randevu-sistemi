using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using HospitalAppointment.Infrastructure.Identity;
using HospitalAppointment.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalAppointment.Infrastructure.Extensions;

// Altyapı katmanı servislerini DI konteynerine kaydeden genişletme metodu
public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // PostgreSQL veritabanı bağlantısı — Npgsql sağlayıcısı
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // ASP.NET Core Identity yapılandırması
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Genel repository kaydı
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        // Özelleştirilmiş repository kayıtları
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IDoctorRepository, DoctorRepository>();
        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IHospitalRepository, HospitalRepository>();
        services.AddScoped<IBranchRepository, BranchRepository>();
        services.AddScoped<IDepartmentRepository, DepartmentRepository>();
        services.AddScoped<IHealthcareServiceRepository, HealthcareServiceRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Kimlik doğrulama servisi
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
