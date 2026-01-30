namespace PHPT.Data.Entities;

public class Patient : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PatientCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Guid FacilityId { get; set; }

    public Facility Facility { get; set; } = null!;
    public PatientWallet? Wallet { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public string FullName => $"{FirstName} {LastName}";
}
