using HospitalAppointment.Domain.Entities.Base;
using HospitalAppointment.Domain.ValueObjects;

namespace HospitalAppointment.Domain.Entities;

// Hastane entity'si — Bileşim (Composition): Hospital -> Branch -> Department hiyerarşisi
public class Hospital : BaseEntity
{
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public string? TaxNumber { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<Branch> _branches = [];
    public IReadOnlyCollection<Branch> Branches => _branches.AsReadOnly();

    private Hospital()
    {
        Name = "";
        Address = Address.Of("", "", "", "");
    }

    public Hospital(string name, Address address, string? taxNumber = null, string? contactEmail = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hastane adı boş olamaz.", nameof(name));
        ArgumentNullException.ThrowIfNull(address);

        Name = name.Trim();
        Address = address;
        TaxNumber = taxNumber?.Trim();
        ContactEmail = contactEmail?.Trim();
        IsActive = true;
    }

    public void Update(string name, Address address, string? contactEmail = null, string? website = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hastane adı boş olamaz.", nameof(name));

        Name = name.Trim();
        Address = address ?? throw new ArgumentNullException(nameof(address));
        ContactEmail = contactEmail?.Trim();
        Website = website?.Trim();
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
