using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using InvoiceSystem.Domain.Services;

namespace InvoiceSystem.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public PaymentService(IPaymentRepository paymentRepository, IInvoiceRepository invoiceRepository)
    {
        _paymentRepository = paymentRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<Payment>> GetPaymentsForInvoiceAsync(int invoiceId)
    {
        return await _paymentRepository.GetAllForInvoiceAsync(invoiceId);
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await _paymentRepository.GetByIdAsync(id);
    }

    public async Task<Payment> RecordPaymentAsync(Payment payment)
    {
        payment.PaymentDate = DateTime.Now;
        var result = await _paymentRepository.AddAsync(payment);
        
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(payment.InvoiceId);
        if (invoice != null)
        {
            invoice.UpdateStatus();
            await _invoiceRepository.UpdateAsync(invoice);
        }
        
        return result;
    }

    public async Task UpdatePaymentAsync(Payment payment)
    {
        await _paymentRepository.UpdateAsync(payment);
        
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(payment.InvoiceId);
        if (invoice != null)
        {
            invoice.UpdateStatus();
            await _invoiceRepository.UpdateAsync(invoice);
        }
    }

    public async Task DeletePaymentAsync(int id)
    {
        var payment = await _paymentRepository.GetByIdAsync(id);
        if (payment != null)
        {
            await _paymentRepository.DeleteAsync(id);
            
            var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(payment.InvoiceId);
            if (invoice != null)
            {
                invoice.UpdateStatus();
                await _invoiceRepository.UpdateAsync(invoice);
            }
        }
    }

    public async Task<decimal> GetTotalPaymentsForInvoiceAsync(int invoiceId)
    {
        var payments = await _paymentRepository.GetAllForInvoiceAsync(invoiceId);
        return payments.Sum(p => p.Amount);
    }
} 