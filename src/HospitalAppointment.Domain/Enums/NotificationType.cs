namespace HospitalAppointment.Domain.Enums;

public enum NotificationType
{
    AppointmentCreated = 0,
    AppointmentConfirmed = 1,
    AppointmentRejected = 2,
    AppointmentCancelled = 3,
    AppointmentReminder = 4,
    AppointmentCompleted = 5
}
