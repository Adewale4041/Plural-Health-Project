using PHPT.Common.Enums;

namespace PHPT.Data.Entities;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;
    public string Notes { get; set; } = string.Empty;
    public Guid FacilityId { get; set; }

    public Patient Patient { get; set; } = null!;
    public Clinic Clinic { get; set; } = null!;
    public Invoice? Invoice { get; set; }

    public DateTime AppointmentDateTime => AppointmentDate.Add(AppointmentTime);
}
