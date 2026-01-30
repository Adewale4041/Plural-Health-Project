using Microsoft.EntityFrameworkCore;
using PHPT.Data.Context;
using PHPT.Data.Entities;
using PHPT.Data.Repositories.Interfaces;

namespace PHPT.Data.Repositories.Implementations;

public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Invoice?> GetInvoiceWithDetailsAsync(Guid invoiceId)
    {
        return await _dbSet
            .Include(i => i.Items)
            .Include(i => i.Patient)
                .ThenInclude(p => p.Wallet)
            .Include(i => i.Appointment)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
    }

    public async Task<Invoice?> GetInvoiceByAppointmentIdAsync(Guid appointmentId)
    {
        return await _dbSet
            .Include(i => i.Items)
            .Include(i => i.Patient)
                .ThenInclude(p => p.Wallet)
            .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"INV{today:yyyyMMdd}";
        
        var lastInvoice = await _dbSet
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        if (lastInvoice == null)
            return $"{prefix}0001";

        var lastNumber = int.Parse(lastInvoice.InvoiceNumber.Substring(prefix.Length));
        return $"{prefix}{(lastNumber + 1):D4}";
    }
}
