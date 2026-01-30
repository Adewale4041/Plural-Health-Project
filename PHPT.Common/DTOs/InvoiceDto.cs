using PHPT.Common.Enums;

namespace PHPT.Common.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid AppointmentId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusDisplay { get; set; } = string.Empty;
    public DateTime? PaidAt { get; set; }
    public List<InvoiceItemDto> Items { get; set; } = new();
}
