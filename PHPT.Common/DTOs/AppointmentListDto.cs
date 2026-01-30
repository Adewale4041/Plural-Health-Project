using PHPT.Common.Enums;

namespace PHPT.Common.DTOs;

public class AppointmentListDto
{
    public Guid Id { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientCode { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public DateTime AppointmentDateTime { get; set; }
    public string AppointmentTime { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public string ClinicName { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string WalletBalanceFormatted { get; set; } = string.Empty;
    public bool HasInvoice { get; set; }
    public Guid? InvoiceId { get; set; }
}
