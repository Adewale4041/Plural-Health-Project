namespace PHPT.Data.Entities;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
