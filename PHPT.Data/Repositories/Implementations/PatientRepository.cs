using Microsoft.EntityFrameworkCore;
using PHPT.Data.Context;
using PHPT.Data.Entities;
using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.Repositories.Implementations;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetPatientWithWalletAsync(Guid patientId)
    {
        return await _dbSet
            .Include(p => p.Wallet)
            .FirstOrDefaultAsync(p => p.Id == patientId);
    }

    public async Task<Patient?> GetByPatientCodeAsync(string patientCode)
    {
        return await _dbSet
            .Include(p => p.Wallet)
            .FirstOrDefaultAsync(p => p.PatientCode == patientCode);
    }
}
