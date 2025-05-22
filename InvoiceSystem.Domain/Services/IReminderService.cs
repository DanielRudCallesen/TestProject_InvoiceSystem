using InvoiceSystem.Domain.Entities;

namespace InvoiceSystem.Domain.Services;

public interface IReminderService
{
    Task<IEnumerable<Reminder>> GetRemindersForInvoiceAsync(int invoiceId);
    Task<Reminder?> GetReminderByIdAsync(int id);
    Task<Reminder> CreateReminderAsync(Reminder reminder);
    Task UpdateReminderAsync(Reminder reminder);
    Task<IEnumerable<Invoice>> GetInvoicesNeedingRemindersAsync(int daysBefore, int daysAfter);
    Task SendReminderAsync(int invoiceId, ReminderType reminderType, string message);
} 