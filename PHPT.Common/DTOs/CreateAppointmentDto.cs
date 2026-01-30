namespace PHPT.Common.DTOs;

public class CreateAppointmentDto
{
    public Guid PatientId { get; set; }
    public Guid ClinicId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
