using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Repositories;

public interface IReminderRepository
{
    Task<IEnumerable<Reminder>> GetAllForInvoiceAsync(int invoiceId);
    Task<Reminder?> GetByIdAsync(int id);
    Task<Reminder> AddAsync(Reminder reminder);
    Task UpdateAsync(Reminder reminder);
    Task DeleteAsync(int id);
} 