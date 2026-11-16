using HospitalAppointment.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HospitalAppointment.Infrastructure.Data;

// Unit of Work implementasyonu — işlem bütünlüğünü yönetir
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        await _transaction!.CommitAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
            await _transaction.RollbackAsync(cancellationToken);
    }

    // PostgreSQL pg_advisory_xact_lock — transaction boyunca geçerli, otomatik serbest bırakılır.
    // Hem doktor hem hasta için kilitler; deadlock önlemek için tutarlı sıralama (küçük key önce).
    public async Task AcquireAppointmentLocksAsync(
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var doctorKey = GuidToLockKey(doctorId);
        var patientKey = GuidToLockKey(patientId);

        // Tutarlı sıralama: küçük key önce → birden fazla transaction aynı sırayı izlerse deadlock oluşmaz
        var (firstKey, secondKey) = doctorKey < patientKey
            ? (doctorKey, patientKey)
            : (patientKey, doctorKey);

        await _context.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_xact_lock({firstKey}), pg_advisory_xact_lock({secondKey})");
    }

    // Raw SQL güncellemesi sonrası EF'in stale state'i temizler.
    // Aynı DbContext kapsamında başka SaveChangesAsync çağrıları etkilenmesin diye.
    public void ClearTracker() => _context.ChangeTracker.Clear();

    // Guid'den deterministik 64-bit lock key üretir.
    // XOR tabanlı — farklı Guid'ler için çakışma olasılığı 1/2^64 (ihmal edilebilir).
    private static long GuidToLockKey(Guid id)
    {
        var bytes = id.ToByteArray();
        return BitConverter.ToInt64(bytes, 0) ^ BitConverter.ToInt64(bytes, 8);
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
