using PHPT.Data.Entities;

namespace PHPT.Data.Repositories.Interfaces;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetPatientWithWalletAsync(Guid patientId);
    Task<Patient?> GetByPatientCodeAsync(string patientCode);
}
