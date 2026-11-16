using HospitalAppointment.Domain.Entities;

namespace HospitalAppointment.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<IReadOnlyList<Notification>> GetByUserIdAsync(string userId, bool unreadOnly = false, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
}
