using PHPT.Data.Entities;

namespace PHPT.Data.Repositories.Interfaces;

public interface IWalletRepository : IRepository<PatientWallet>
{
    Task<PatientWallet?> GetWalletByPatientIdAsync(Guid patientId);
    Task<WalletTransaction> AddTransactionAsync(WalletTransaction transaction);
}
