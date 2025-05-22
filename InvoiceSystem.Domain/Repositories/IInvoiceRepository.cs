using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Repositories;

public interface IInvoiceRepository
{
    Task<IEnumerable<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();
    Task<IEnumerable<Invoice>> GetInvoicesDueSoonAsync(int daysThreshold);
    Task<Invoice> AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
    Task DeleteAsync(int id);
} 