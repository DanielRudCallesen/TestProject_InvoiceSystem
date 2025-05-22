using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Repositories;

public interface IPaymentRepository
{
    Task<IEnumerable<Payment>> GetAllForInvoiceAsync(int invoiceId);
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment> AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task DeleteAsync(int id);
} 