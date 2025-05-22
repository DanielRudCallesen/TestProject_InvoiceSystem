using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Services;

public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetPaymentsForInvoiceAsync(int invoiceId);
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<Payment> RecordPaymentAsync(Payment payment);
    Task UpdatePaymentAsync(Payment payment);
    Task DeletePaymentAsync(int id);
    Task<decimal> GetTotalPaymentsForInvoiceAsync(int invoiceId);
} 