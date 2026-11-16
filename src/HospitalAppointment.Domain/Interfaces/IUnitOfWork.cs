namespace HospitalAppointment.Domain.Interfaces;

// Unit of Work — işlem bütünlüğünü korur
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    // PostgreSQL transaction-scoped advisory lock.
    // Deadlock riskini önlemek için birden fazla lock her zaman aynı sırada alınmalıdır.
    // doctorId ve patientId her ikisi de kilitlenir; küçük lock key önce alınır.
    Task AcquireAppointmentLocksAsync(Guid doctorId, Guid patientId, CancellationToken cancellationToken = default);

    // EF change tracker'ı temizle — raw SQL güncelleme sonrası gereklidir
    void ClearTracker();
}
