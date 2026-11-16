namespace HospitalAppointment.Domain.Exceptions;

public class AppointmentConflictException : DomainException
{
    public AppointmentConflictException(string message)
        : base(message, "APPOINTMENT_CONFLICT") { }
}
