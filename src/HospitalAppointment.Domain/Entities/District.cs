using HospitalAppointment.Domain.Entities.Base;

namespace HospitalAppointment.Domain.Entities;

// Türkiye ilçesi — bir ile bağlı
public class District : BaseEntity
{
    public Guid CityId { get; private set; }
    public string Name { get; private set; }

    public City? City { get; private set; }

    private District() { Name = string.Empty; }

    public District(Guid cityId, string name)
    {
        if (cityId == Guid.Empty)
            throw new ArgumentException("İl kimliği geçersiz.", nameof(cityId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("İlçe adı boş olamaz.", nameof(name));

        CityId = cityId;
        Name = name.Trim();
    }
}
