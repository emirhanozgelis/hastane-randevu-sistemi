using HospitalAppointment.Domain.Entities.Base;

namespace HospitalAppointment.Domain.Entities;

// Doktor müsaitlik takvimi — haftalık program veya özel tarih bazlı
public class DoctorAvailability : BaseEntity
{
    public Guid DoctorId { get; private set; }

    // DateOnly.MinValue ise haftalık program, dolu ise özel tarih geçersizliği
    public DateOnly Date { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }

    // false ise doktor o gün izinli veya tatilde
    public bool IsAvailable { get; private set; }
    public string? Notes { get; private set; }

    public Doctor? Doctor { get; private set; }

    private DoctorAvailability() { }

    // Haftalık tekrarlayan program için
    public static DoctorAvailability CreateWeeklySchedule(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        ValidateTimes(startTime, endTime);
        return new DoctorAvailability
        {
            DoctorId = doctorId,
            Date = DateOnly.MinValue,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsAvailable = true
        };
    }

    // Belirli bir tarih için özel program veya izin günü
    public static DoctorAvailability CreateDateOverride(
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        bool isAvailable = true,
        string? notes = null)
    {
        if (!isAvailable == false)
            ValidateTimes(startTime, endTime);

        return new DoctorAvailability
        {
            DoctorId = doctorId,
            Date = date,
            DayOfWeek = date.DayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            IsAvailable = isAvailable,
            Notes = notes?.Trim()
        };
    }

    // İzin/tatil günü
    public static DoctorAvailability CreateLeaveDay(Guid doctorId, DateOnly date, string? reason = null)
    {
        return new DoctorAvailability
        {
            DoctorId = doctorId,
            Date = date,
            DayOfWeek = date.DayOfWeek,
            StartTime = TimeOnly.MinValue,
            EndTime = TimeOnly.MinValue,
            IsAvailable = false,
            Notes = reason?.Trim()
        };
    }

    public bool IsSpecificDate => Date != DateOnly.MinValue;

    public void Update(TimeOnly startTime, TimeOnly endTime, bool isAvailable, string? notes = null)
    {
        if (isAvailable) ValidateTimes(startTime, endTime);
        StartTime = startTime;
        EndTime = endTime;
        IsAvailable = isAvailable;
        Notes = notes?.Trim();
        MarkUpdated();
    }

    private static void ValidateTimes(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Başlangıç saati bitiş saatinden önce olmalıdır.");
    }
}
