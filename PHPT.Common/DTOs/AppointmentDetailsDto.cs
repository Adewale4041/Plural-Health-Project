using PHPT.Common.Enums;

namespace PHPT.Common.DTOs;

public class AppointmentDetailsDto
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientCode { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan AppointmentTime { get; set; }
    public DateTime AppointmentDateTime { get; set; }
    public string AppointmentType { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public InvoiceDto? Invoice { get; set; }
}
