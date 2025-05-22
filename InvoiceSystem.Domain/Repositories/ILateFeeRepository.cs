using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Repositories;

public interface ILateFeeRepository
{
    Task<IEnumerable<LateFee>> GetAllForInvoiceAsync(int invoiceId);
    Task<LateFee?> GetByIdAsync(int id);
    Task<LateFee> AddAsync(LateFee lateFee);
    Task UpdateAsync(LateFee lateFee);
    Task DeleteAsync(int id);
} 