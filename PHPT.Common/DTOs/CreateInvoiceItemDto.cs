namespace PHPT.Common.DTOs;

public class CreateInvoiceItemDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}
