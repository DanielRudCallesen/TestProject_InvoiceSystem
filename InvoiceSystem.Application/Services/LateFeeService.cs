using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using InvoiceSystem.Domain.Services;

namespace InvoiceSystem.Application.Services;

public class LateFeeService : ILateFeeService
{
    private readonly ILateFeeRepository _lateFeeRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public LateFeeService(ILateFeeRepository lateFeeRepository, IInvoiceRepository invoiceRepository)
    {
        _lateFeeRepository = lateFeeRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<LateFee>> GetLateFeesForInvoiceAsync(int invoiceId)
    {
        return await _lateFeeRepository.GetAllForInvoiceAsync(invoiceId);
    }

    public async Task<LateFee?> GetLateFeeByIdAsync(int id)
    {
        return await _lateFeeRepository.GetByIdAsync(id);
    }

    public async Task<LateFee> ApplyLateFeeAsync(LateFee lateFee)
    {
        lateFee.AppliedDate = DateTime.Now;
        return await _lateFeeRepository.AddAsync(lateFee);
    }

    public async Task<decimal> CalculateLateFeeAsync(int invoiceId, DateTime currentDate, decimal feePercentage)
    {
        var invoice = await _invoiceRepository.GetByIdWithDetailsAsync(invoiceId);
        if (invoice == null || !invoice.IsOverdue())
        {
            return 0;
        }

        var daysOverdue = (currentDate.Date - invoice.DueDate.Date).Days;
        if (daysOverdue <= 0)
        {
            return 0;
        }

        // Apply fee based on remaining amount
        var remainingAmount = invoice.GetRemainingAmount();
        return Math.Round(remainingAmount * (feePercentage / 100), 2);
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesForLateFeeApplicationAsync()
    {
        var overdueInvoices = await _invoiceRepository.GetOverdueInvoicesAsync();
        var result = new List<Invoice>();
        
        foreach (var invoice in overdueInvoices)
        {
            // Only apply late fees to invoices that haven't had a late fee applied today
            var lateFees = await _lateFeeRepository.GetAllForInvoiceAsync(invoice.Id);
            var hasLateFeeToday = lateFees.Any(lf => lf.AppliedDate.Date == DateTime.Now.Date);
            
            if (!hasLateFeeToday)
            {
                result.Add(invoice);
            }
        }
        
        return result;
    }
} 