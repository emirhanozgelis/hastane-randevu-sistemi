using HospitalAppointment.Domain.Enums;

namespace HospitalAppointment.Application.Interfaces;

// Bildirim servis arayüzü — OCP: yeni bildirim kanalı eklenirken bu interface değişmez
public interface INotificationService
{
    Task SendAppointmentCreatedAsync(string userId, Guid appointmentId, string serviceName, DateOnly date, TimeOnly time, CancellationToken cancellationToken = default);
    Task SendAppointmentConfirmedAsync(string userId, Guid appointmentId, string doctorName, DateOnly date, TimeOnly time, CancellationToken cancellationToken = default);
    Task SendAppointmentRejectedAsync(string userId, Guid appointmentId, string reason, CancellationToken cancellationToken = default);
    Task SendAppointmentCancelledAsync(string userId, Guid appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationSummaryDto>> GetUserNotificationsAsync(string userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
}

public record NotificationSummaryDto(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    NotificationType Type,
    Guid? RelatedAppointmentId,
    DateTime CreatedAt
);
