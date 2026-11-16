using HospitalAppointment.Domain.Entities.Base;

namespace HospitalAppointment.Domain.Entities;

// Türkiye ili — plaka kodu ve ilçe listesiyle
public class City : BaseEntity
{
    public int PlateCode { get; private set; }
    public string Name { get; private set; }

    private readonly List<District> _districts = [];
    public IReadOnlyCollection<District> Districts => _districts.AsReadOnly();

    private City() { Name = string.Empty; }

    public City(int plateCode, string name)
    {
        if (plateCode < 1 || plateCode > 81)
            throw new ArgumentException("Plaka kodu 1-81 arasında olmalıdır.", nameof(plateCode));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("İl adı boş olamaz.", nameof(name));

        PlateCode = plateCode;
        Name = name.Trim();
    }
}
