using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Services;

public interface ILateFeeService
{
    Task<IEnumerable<LateFee>> GetLateFeesForInvoiceAsync(int invoiceId);
    Task<LateFee?> GetLateFeeByIdAsync(int id);
    Task<LateFee> ApplyLateFeeAsync(LateFee lateFee);
    Task<decimal> CalculateLateFeeAsync(int invoiceId, DateTime currentDate, decimal feePercentage);
    Task<IEnumerable<Invoice>> GetInvoicesForLateFeeApplicationAsync();
} 