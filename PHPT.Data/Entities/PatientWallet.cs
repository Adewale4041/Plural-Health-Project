using PHPT.Common.Constants;

namespace PHPT.Data.Entities;

public class PatientWallet : BaseEntity
{
    public Guid PatientId { get; set; }
    public decimal Balance { get; set; }
    public string Currency { get; set; } = AppConstants.DefaultCurrency;
    public DateTime LastTransactionDate { get; set; }

    public Patient Patient { get; set; } = null!;
    public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
}
