using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using InvoiceSystem.Domain.Services;

namespace InvoiceSystem.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
    {
        return await _invoiceRepository.GetAllAsync();
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int id)
    {
        return await _invoiceRepository.GetByIdAsync(id);
    }

    public async Task<Invoice?> GetInvoiceWithDetailsAsync(int id)
    {
        return await _invoiceRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        if (invoice.Amount <= 0 || invoice.Amount > 100)
        {
            throw new ArgumentException("Invoice amount must be between 0 and 100");
        }
        invoice.CreatedDate = DateTime.Now;
        invoice.Status = InvoiceStatus.Pending;
        return await _invoiceRepository.AddAsync(invoice);
    }

    public async Task UpdateInvoiceAsync(Invoice invoice)
    {
        invoice.UpdateStatus();
        await _invoiceRepository.UpdateAsync(invoice);
    }

    public async Task DeleteInvoiceAsync(int id)
    {
        await _invoiceRepository.DeleteAsync(id);
    }

    public async Task<decimal> CalculateRemainingAmountAsync(int invoiceId)
    {
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
        return invoice?.GetRemainingAmount() ?? 0;
    }

    public async Task<bool> IsInvoicePaidAsync(int invoiceId)
    {
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
        return invoice?.IsPaid() ?? false;
    }

    public async Task<bool> IsInvoiceOverdueAsync(int invoiceId)
    {
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
        return invoice?.IsOverdue() ?? false;
    }
} 