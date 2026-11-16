using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Domain.Entities;

// Kullanıcı bildirimi entity'si
public class Notification : BaseEntity
{
    public string UserId { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public bool IsRead { get; private set; }
    public NotificationType Type { get; private set; }
    public Guid? RelatedAppointmentId { get; private set; }

    private Notification() { UserId = ""; Title = ""; Message = ""; }

    public Notification(
        string userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedAppointmentId = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("Kullanıcı kimliği boş olamaz.", nameof(userId));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Başlık boş olamaz.", nameof(title));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Mesaj boş olamaz.", nameof(message));

        UserId = userId;
        Title = title.Trim();
        Message = message.Trim();
        Type = type;
        IsRead = false;
        RelatedAppointmentId = relatedAppointmentId;
    }

    public void MarkAsRead()
    {
        IsRead = true;
        MarkUpdated();
    }
}
