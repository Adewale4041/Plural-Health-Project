namespace PHPT.Data.Entities;

public class Facility : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}
