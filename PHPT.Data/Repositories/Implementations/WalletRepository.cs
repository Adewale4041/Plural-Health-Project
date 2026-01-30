using Microsoft.EntityFrameworkCore;
using PHPT.Data.Context;
using PHPT.Data.Entities;
using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.Repositories.Implementations;

public class WalletRepository : Repository<PatientWallet>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PatientWallet?> GetWalletByPatientIdAsync(Guid patientId)
    {
        return await _dbSet
            .Include(w => w.Patient)
            .FirstOrDefaultAsync(w => w.PatientId == patientId);
    }

    public async Task<WalletTransaction> AddTransactionAsync(WalletTransaction transaction)
    {
        await _context.WalletTransactions.AddAsync(transaction);
        return transaction;
    }
}
