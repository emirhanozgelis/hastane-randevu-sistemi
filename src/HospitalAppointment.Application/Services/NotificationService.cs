using HospitalAppointment.Application.Interfaces;
using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Enums;
using HospitalAppointment.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Application.Services;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SendAppointmentCreatedAsync(
        string userId,
        Guid appointmentId,
        string serviceName,
        DateOnly date,
        TimeOnly time,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu oluşturma bildirimi gönderiliyor — Kullanıcı: {UserId}", userId);

        var notification = new Notification(
            userId: userId,
            title: "Randevunuz Oluşturuldu",
            message: $"{date:dd.MM.yyyy} tarihinde saat {time:HH:mm}'de '{serviceName}' için randevunuz başarıyla oluşturuldu. Onay bekleniyor.",
            type: NotificationType.AppointmentCreated,
            relatedAppointmentId: appointmentId);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendAppointmentConfirmedAsync(
        string userId,
        Guid appointmentId,
        string doctorName,
        DateOnly date,
        TimeOnly time,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu onay bildirimi gönderiliyor — Kullanıcı: {UserId}", userId);

        var notification = new Notification(
            userId: userId,
            title: "Randevunuz Onaylandı",
            message: $"{date:dd.MM.yyyy} tarihinde saat {time:HH:mm}'de {doctorName} ile olan randevunuz onaylandı.",
            type: NotificationType.AppointmentConfirmed,
            relatedAppointmentId: appointmentId);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendAppointmentRejectedAsync(
        string userId,
        Guid appointmentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu red bildirimi gönderiliyor — Kullanıcı: {UserId}", userId);

        var notification = new Notification(
            userId: userId,
            title: "Randevunuz Reddedildi",
            message: $"Randevunuz reddedildi. Neden: {reason}",
            type: NotificationType.AppointmentRejected,
            relatedAppointmentId: appointmentId);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SendAppointmentCancelledAsync(
        string userId,
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Randevu iptal bildirimi gönderiliyor — Kullanıcı: {UserId}", userId);

        var notification = new Notification(
            userId: userId,
            title: "Randevunuz İptal Edildi",
            message: "Randevunuz iptal edildi. Yeni randevu oluşturmak için sistemi kullanabilirsiniz.",
            type: NotificationType.AppointmentCancelled,
            relatedAppointmentId: appointmentId);

        await _notificationRepository.AddAsync(notification, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<NotificationSummaryDto>> GetUserNotificationsAsync(
        string userId,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationRepository.GetByUserIdAsync(userId, unreadOnly, cancellationToken);

        return notifications
            .Select(MapToDto)
            .ToList()
            .AsReadOnly();
    }

    public async Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tüm bildirimler okundu olarak işaretleniyor — Kullanıcı: {UserId}", userId);

        await _notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static NotificationSummaryDto MapToDto(Notification n) => new(
        Id: n.Id,
        Title: n.Title,
        Message: n.Message,
        IsRead: n.IsRead,
        Type: n.Type,
        RelatedAppointmentId: n.RelatedAppointmentId,
        CreatedAt: n.CreatedAt);
}
