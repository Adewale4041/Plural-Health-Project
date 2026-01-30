namespace PHPT.Data.Entities;

public class WalletTransaction : BaseEntity
{
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? InvoiceId { get; set; }

    public PatientWallet Wallet { get; set; } = null!;
    public Invoice? Invoice { get; set; }
}
