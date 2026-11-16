namespace HospitalAppointment.Application.DTOs.Appointment;

// Bir doktorun belirli gün ve hizmet için müsait randevu slotu
public record AvailableSlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime,
    string Display,        // "09:00 - 09:30"
    string StartTimeValue  // "09:00:00" — form value olarak gönderilir
);
