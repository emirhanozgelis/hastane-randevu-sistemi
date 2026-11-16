using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Domain.Interfaces;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Infrastructure.Repositories;

// Bildirim repository implementasyonu
public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context) { }

    // Kullanıcıya ait bildirimleri getir, isteğe bağlı okunmamış filtresi
    public async Task<IReadOnlyList<Notification>> GetByUserIdAsync(
        string userId,
        bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    // Kullanıcının tüm bildirimlerini okundu olarak işaretle
    public async Task MarkAllAsReadAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        // Okunmamış bildirimleri toplu güncelle
        await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(n => n.IsRead, true)
                    .SetProperty(n => n.UpdatedAt, DateTime.UtcNow),
                cancellationToken);
    }
}
