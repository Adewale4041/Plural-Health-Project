using PHPT.Common.DTOs;
using PHPT.Common.DTOs;

namespace PHPT.Business.Services.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceDto> CreateInvoiceAsync(Guid facilityId, CreateInvoiceDto dto);
    Task<InvoiceDto> PayInvoiceAsync(Guid facilityId, PayInvoiceDto dto);
    Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, Guid facilityId);
}
