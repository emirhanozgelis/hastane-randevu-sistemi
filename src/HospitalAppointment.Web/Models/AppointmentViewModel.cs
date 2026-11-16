using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HospitalAppointment.Web.Models;

// Randevu oluşturma formu view model'i
public class AppointmentViewModel
{
    [Required(ErrorMessage = "Hastane seçimi zorunludur.")]
    [Display(Name = "Hastane")]
    public Guid HospitalId { get; set; }

    [Required(ErrorMessage = "Şube seçimi zorunludur.")]
    [Display(Name = "Şube")]
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "Bölüm seçimi zorunludur.")]
    [Display(Name = "Bölüm")]
    public Guid DepartmentId { get; set; }

    [Required(ErrorMessage = "Doktor seçimi zorunludur.")]
    [Display(Name = "Doktor")]
    public Guid DoctorId { get; set; }

    [Required(ErrorMessage = "Hizmet seçimi zorunludur.")]
    [Display(Name = "Hizmet")]
    public Guid HealthcareServiceId { get; set; }

    [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Randevu Tarihi")]
    public DateOnly AppointmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddDays(1));

    // Saat — müsaitlik grid'inden seçilir, "HH:mm:ss" formatında gelir
    [Required(ErrorMessage = "Randevu saati zorunludur.")]
    [Display(Name = "Başlangıç Saati")]
    public TimeOnly StartTime { get; set; }

    [Display(Name = "Notlar")]
    [StringLength(500)]
    public string? Notes { get; set; }

    // Dropdown listeler
    public SelectList? Hospitals { get; set; }
    public SelectList? Branches { get; set; }
    public SelectList? Departments { get; set; }
    public SelectList? Doctors { get; set; }
    public SelectList? Services { get; set; }
}
