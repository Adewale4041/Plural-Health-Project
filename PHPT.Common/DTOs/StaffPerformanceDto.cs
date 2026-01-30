namespace PHPT.Common.DTOs;

public class StaffPerformanceDto
{
    public Guid StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int AppointmentsCreated { get; set; }
    public int InvoicesCreated { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
    public int PaymentsProcessed { get; set; }
    public decimal TotalPaymentAmount { get; set; }
    public string Currency { get; set; } = "NGN";
}
