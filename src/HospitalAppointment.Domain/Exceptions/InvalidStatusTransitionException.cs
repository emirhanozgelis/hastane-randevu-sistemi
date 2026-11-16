using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Domain.Exceptions;

public class InvalidStatusTransitionException : DomainException
{
    public AppointmentStatus FromStatus { get; }
    public AppointmentStatus ToStatus { get; }

    public InvalidStatusTransitionException(AppointmentStatus from, AppointmentStatus to)
        : base($"Randevu durumu '{from}' konumundan '{to}' konumuna geçirilemiyor.", "INVALID_STATUS_TRANSITION")
    {
        FromStatus = from;
        ToStatus = to;
    }
}
