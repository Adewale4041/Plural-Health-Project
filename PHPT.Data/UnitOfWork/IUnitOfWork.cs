using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.UnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IAppointmentRepository Appointments { get; }
    IPatientRepository Patients { get; }
    IInvoiceRepository Invoices { get; }
    IWalletRepository Wallets { get; }
    IClinicRepository Clinics { get; }
    IFacilityRepository Facilities { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
