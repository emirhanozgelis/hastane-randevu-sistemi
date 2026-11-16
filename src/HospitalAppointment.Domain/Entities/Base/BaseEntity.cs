namespace HospitalAppointment.Domain.Entities.Base;

// Tüm entity'ler için ortak kimlik ve zaman damgası alanları
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    protected void MarkUpdated() => UpdatedAt = DateTime.UtcNow;
}
