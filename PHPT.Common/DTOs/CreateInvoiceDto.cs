namespace PHPT.Common.DTOs;

public class CreateInvoiceDto
{
    public Guid AppointmentId { get; set; }
    public decimal DiscountPercentage { get; set; }
    public List<CreateInvoiceItemDto> Items { get; set; } = new();
}
