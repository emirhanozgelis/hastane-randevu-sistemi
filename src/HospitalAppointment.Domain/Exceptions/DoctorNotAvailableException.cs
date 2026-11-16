namespace HospitalAppointment.Domain.Exceptions;

public class DoctorNotAvailableException : DomainException
{
    public DoctorNotAvailableException(string doctorName, DateOnly date, TimeOnly startTime)
        : base($"Dr. {doctorName} {date:dd.MM.yyyy} tarihinde {startTime:HH:mm} saatinde müsait değil.", "DOCTOR_NOT_AVAILABLE") { }
}
