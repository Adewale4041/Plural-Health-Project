using PHPT.Data.Entities;

namespace PHPT.Data.Repositories.Interfaces;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetInvoiceWithDetailsAsync(Guid invoiceId);
    Task<Invoice?> GetInvoiceByAppointmentIdAsync(Guid appointmentId);
    Task<string> GenerateInvoiceNumberAsync();
}
