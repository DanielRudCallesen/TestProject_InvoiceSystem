using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Services;

public interface IInvoiceService
{
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
    Task<Invoice?> GetInvoiceByIdAsync(int id);
    Task<Invoice?> GetInvoiceWithDetailsAsync(int id);
    Task<Invoice> CreateInvoiceAsync(Invoice invoice);
    Task UpdateInvoiceAsync(Invoice invoice);
    Task DeleteInvoiceAsync(int id);
    Task<decimal> CalculateRemainingAmountAsync(int invoiceId);
    Task<bool> IsInvoicePaidAsync(int invoiceId);
    Task<bool> IsInvoiceOverdueAsync(int invoiceId);
} 