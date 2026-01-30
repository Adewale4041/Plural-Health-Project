using PHPT.Common.Enums;

namespace PHPT.Data.Entities;

public class Invoice : BaseEntity
{
    public Guid PatientId { get; set; }
    public Guid AppointmentId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Unpaid;
    public DateTime? PaidAt { get; set; }
    public Guid FacilityId { get; set; }

    public Patient Patient { get; set; } = null!;
    public Appointment Appointment { get; set; } = null!;
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
