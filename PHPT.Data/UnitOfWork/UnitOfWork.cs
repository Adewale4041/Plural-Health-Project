using Microsoft.EntityFrameworkCore.Storage;
using PHPT.Data.Context;
using PHPT.Data.Repositories.Implementations;
using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public IAppointmentRepository Appointments { get; }
    public IPatientRepository Patients { get; }
    public IInvoiceRepository Invoices { get; }
    public IWalletRepository Wallets { get; }
    public IClinicRepository Clinics { get; }
    public IFacilityRepository Facilities { get; }

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Appointments = new AppointmentRepository(_context);
        Patients = new PatientRepository(_context);
        Invoices = new InvoiceRepository(_context);
        Wallets = new WalletRepository(_context);
        Clinics = new ClinicRepository(_context);
        Facilities = new FacilityRepository(_context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
