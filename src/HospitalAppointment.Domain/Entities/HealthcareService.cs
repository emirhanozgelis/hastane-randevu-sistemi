using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.ValueObjects;

namespace HospitalAppointment.Domain.Entities;

// Sağlık hizmeti entity'si — Money value object kullanımı (kapsülleme)
public class HealthcareService : BaseEntity
{
    public Guid DepartmentId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public TimeSpan Duration { get; private set; }
    public Money Price { get; private set; }
    public bool IsActive { get; private set; }

    public Department? Department { get; private set; }

    private HealthcareService()
    {
        Name = "";
        Price = Money.Zero();
    }

    public HealthcareService(
        Guid departmentId,
        string name,
        TimeSpan duration,
        Money price,
        string? description = null)
    {
        if (departmentId == Guid.Empty)
            throw new ArgumentException("Bölüm kimliği geçersiz.", nameof(departmentId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hizmet adı boş olamaz.", nameof(name));
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Hizmet süresi sıfırdan büyük olmalıdır.", nameof(duration));
        ArgumentNullException.ThrowIfNull(price);

        DepartmentId = departmentId;
        Name = name.Trim();
        Duration = duration;
        Price = price;
        Description = description?.Trim();
        IsActive = true;
    }

    public void UpdatePrice(Money newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);
        Price = newPrice;
        MarkUpdated();
    }

    public void Update(string name, TimeSpan duration, Money price, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hizmet adı boş olamaz.", nameof(name));
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Hizmet süresi sıfırdan büyük olmalıdır.", nameof(duration));

        Name = name.Trim();
        Duration = duration;
        Price = price ?? throw new ArgumentNullException(nameof(price));
        Description = description?.Trim();
        MarkUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkUpdated();
    }
}
