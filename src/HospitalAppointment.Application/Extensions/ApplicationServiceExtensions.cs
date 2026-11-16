using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalAppointment.Application.Extensions;

// Uygulama katmanı servislerini DI konteynerine kaydeden genişletme metodu
public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IDoctorService, DoctorService>();
        services.AddScoped<IHospitalService, HospitalService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<IHealthcareServiceService, HealthcareServiceService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ILocationService, LocationService>();

        return services;
    }
}
