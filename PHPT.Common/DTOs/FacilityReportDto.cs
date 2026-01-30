namespace PHPT.Common.DTOs;

public class FacilityReportDto
{
    public Guid FacilityId { get; set; }
    public string FacilityName { get; set; } = string.Empty;
    public int TotalAppointments { get; set; }
    public int ScheduledAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public int TotalInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalUnpaidAmount { get; set; }
    public int TotalPatients { get; set; }
    public int ActiveStaff { get; set; }
    public string Currency { get; set; } = "NGN";
}
