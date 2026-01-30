namespace PHPT.Common.DTOs;

public class InvoiceItemDto
{
    public Guid Id { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
