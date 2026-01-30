using Microsoft.Extensions.Logging;
using PHPT.Common.DTOs;
using PHPT.Business.Services.Interfaces;
using PHPT.Common.Enums;
using PHPT.Data.Entities;
using PHPT.Data.UnitOfWork;

namespace PHPT.Business.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IUnitOfWork unitOfWork, ILogger<InvoiceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }


    public async Task<InvoiceDto> CreateInvoiceAsync(Guid facilityId, CreateInvoiceDto dto)
    {
        try
        {
            _logger.LogInformation("Creating invoice for appointment {AppointmentId} at facility {FacilityId}", dto.AppointmentId, facilityId);

            // Verify facility exists
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            if (!facility.IsActive)
            {
                throw new InvalidOperationException("Facility is not available or Inactive");
            }

            var appointment = await _unitOfWork.Appointments.GetAppointmentWithDetailsAsync(dto.AppointmentId);

            if (appointment == null || appointment.FacilityId != facilityId)
            {
                throw new InvalidOperationException("Appointment not found or doesn't belong to this facility");
            }

            if (appointment.Invoice != null)
            {
                throw new InvalidOperationException("An invoice already exists for this appointment");
            }

            if (appointment.Status != AppointmentStatus.Scheduled)
            {
                throw new InvalidOperationException("Can only create invoice for scheduled appointments");
            }

            if (!dto.Items.Any())
            {
                throw new InvalidOperationException("Invoice must have at least one item");
            }

            await _unitOfWork.BeginTransactionAsync();

            var subtotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
            var discountAmount = subtotal * (dto.DiscountPercentage / 100);
            var totalAmount = subtotal - discountAmount;

            var invoiceNumber = await _unitOfWork.Invoices.GenerateInvoiceNumberAsync();

            var invoice = new Invoice
            {
                PatientId = appointment.PatientId,
                AppointmentId = appointment.Id,
                InvoiceNumber = invoiceNumber,
                Subtotal = subtotal,
                DiscountPercentage = dto.DiscountPercentage,
                DiscountAmount = discountAmount,
                TotalAmount = totalAmount,
                Status = InvoiceStatus.Unpaid,
                FacilityId = facilityId
            };

            // Create invoice items BEFORE adding invoice
            var items = dto.Items.Select(i => new InvoiceItem
            {
                ServiceName = i.ServiceName,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.Quantity * i.UnitPrice
            }).ToList();

            // Add items to invoice
            foreach (var item in items)
            {
                invoice.Items.Add(item);
            }

            // Add invoice ONCE with all items
            await _unitOfWork.Invoices.AddAsync(invoice);

            // Update appointment status
            appointment.Status = AppointmentStatus.Invoiced;
            appointment.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Appointments.Update(appointment);

            // Save everything in one go
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully created invoice {InvoiceNumber} for appointment {AppointmentId}", invoiceNumber, dto.AppointmentId);

            return (await GetInvoiceByIdAsync(invoice.Id, facilityId))!;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error creating invoice for appointment {AppointmentId}", dto.AppointmentId);
            throw;
        }
    }




    public async Task<InvoiceDto> PayInvoiceAsync(Guid facilityId, PayInvoiceDto dto)
    {
        try
        {
            _logger.LogInformation("Processing payment for invoice {InvoiceId} at facility {FacilityId}", dto.InvoiceId, facilityId);

            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            var invoice = await _unitOfWork.Invoices.GetInvoiceWithDetailsAsync(dto.InvoiceId);
            
            if (invoice == null || invoice.FacilityId != facilityId)
            {
                throw new InvalidOperationException("Invoice not found or doesn't belong to this facility");
            }

            if (invoice.Status != InvoiceStatus.Unpaid)
            {
                throw new InvalidOperationException("Invoice is not in unpaid status");
            }

            var wallet = await _unitOfWork.Wallets.GetWalletByPatientIdAsync(invoice.PatientId);
            
            if (wallet == null)
            {
                throw new InvalidOperationException("Patient wallet not found");
            }

            if (wallet.Balance < invoice.TotalAmount)
            {
                throw new InvalidOperationException($"Insufficient wallet balance. Available: {wallet.Balance:N2}, Required: {invoice.TotalAmount:N2}");
            }

            await _unitOfWork.BeginTransactionAsync();

            var balanceBefore = wallet.Balance;
            wallet.Balance -= invoice.TotalAmount;
            wallet.LastTransactionDate = DateTime.UtcNow;
            wallet.UpdatedAt = DateTime.UtcNow;

            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = invoice.TotalAmount,
                TransactionType = "Payment",
                Description = $"Payment for invoice {invoice.InvoiceNumber}",
                Reference = invoice.InvoiceNumber,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                InvoiceId = invoice.Id
            };

            await _unitOfWork.Wallets.AddTransactionAsync(transaction);
            _unitOfWork.Wallets.Update(wallet);

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            invoice.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Invoices.Update(invoice);

            var appointment = await _unitOfWork.Appointments.GetByIdAsync(invoice.AppointmentId);
            if (appointment != null)
            {
                appointment.Status = AppointmentStatus.Paid;
                appointment.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Appointments.Update(appointment);
            }

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Successfully processed payment for invoice {InvoiceNumber}. Amount: {Amount}, New Balance: {Balance}",
                invoice.InvoiceNumber, invoice.TotalAmount, wallet.Balance);

            return (await GetInvoiceByIdAsync(invoice.Id, facilityId))!;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error processing payment for invoice {InvoiceId}", dto.InvoiceId);
            throw;
        }
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(Guid invoiceId, Guid facilityId)
    {
        try
        {
            var facility = await _unitOfWork.Facilities.GetByIdAsync(facilityId);
            if (facility == null)
            {
                throw new InvalidOperationException("Facility not found");
            }

            var invoice = await _unitOfWork.Invoices.GetInvoiceWithDetailsAsync(invoiceId);
            
            if (invoice == null || invoice.FacilityId != facilityId)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found or doesn't belong to facility {FacilityId}", invoiceId, facilityId);
                return null;
            }

            return new InvoiceDto
            {
                Id = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                PatientId = invoice.PatientId,
                PatientName = invoice.Patient.FullName,
                AppointmentId = invoice.AppointmentId,
                Subtotal = invoice.Subtotal,
                DiscountAmount = invoice.DiscountAmount,
                DiscountPercentage = invoice.DiscountPercentage,
                TotalAmount = invoice.TotalAmount,
                Status = invoice.Status,
                StatusDisplay = invoice.Status.ToString(),
                PaidAt = invoice.PaidAt,
                Items = invoice.Items.Select(i => new InvoiceItemDto
                {
                    Id = i.Id,
                    ServiceName = i.ServiceName,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    TotalPrice = i.TotalPrice
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting invoice {InvoiceId}", invoiceId);
            throw;
        }
    }
}
