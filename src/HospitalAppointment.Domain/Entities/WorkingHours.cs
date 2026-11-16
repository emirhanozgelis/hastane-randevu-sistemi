using HospitalAppointment.Domain.Entities.Base;

namespace HospitalAppointment.Domain.Entities;

// Şube çalışma saatleri — Branch ile Composition
public class WorkingHours : BaseEntity
{
    public Guid BranchId { get; private set; }
    public DayOfWeek Day { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public bool IsClosed { get; private set; }

    public Branch? Branch { get; private set; }

    private WorkingHours() { }

    public WorkingHours(Guid branchId, DayOfWeek day, TimeOnly openTime, TimeOnly closeTime, bool isClosed = false)
    {
        if (branchId == Guid.Empty)
            throw new ArgumentException("Şube kimliği geçersiz.", nameof(branchId));
        if (!isClosed && openTime >= closeTime)
            throw new ArgumentException("Açılış saati kapanış saatinden önce olmalıdır.");

        BranchId = branchId;
        Day = day;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    public void Update(TimeOnly openTime, TimeOnly closeTime, bool isClosed)
    {
        if (!isClosed && openTime >= closeTime)
            throw new ArgumentException("Açılış saati kapanış saatinden önce olmalıdır.");

        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
        MarkUpdated();
    }

    public bool IsWithinWorkingHours(TimeOnly startTime, TimeOnly endTime)
    {
        if (IsClosed) return false;
        return startTime >= OpenTime && endTime <= CloseTime;
    }
}
