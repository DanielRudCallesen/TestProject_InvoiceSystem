using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using InvoiceSystem.Domain.Services;

namespace InvoiceSystem.Application.Services;

public class ReminderService : IReminderService
{
    private readonly IReminderRepository _reminderRepository;
    private readonly IInvoiceRepository _invoiceRepository;

    public ReminderService(IReminderRepository reminderRepository, IInvoiceRepository invoiceRepository)
    {
        _reminderRepository = reminderRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<IEnumerable<Reminder>> GetRemindersForInvoiceAsync(int invoiceId)
    {
        return await _reminderRepository.GetAllForInvoiceAsync(invoiceId);
    }

    public async Task<Reminder?> GetReminderByIdAsync(int id)
    {
        return await _reminderRepository.GetByIdAsync(id);
    }

    public async Task<Reminder> CreateReminderAsync(Reminder reminder)
    {
        reminder.SentDate = DateTime.Now;
        return await _reminderRepository.AddAsync(reminder);
    }

    public async Task UpdateReminderAsync(Reminder reminder)
    {
        await _reminderRepository.UpdateAsync(reminder);
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesNeedingRemindersAsync(int daysBefore, int daysAfter)
    {
        var today = DateTime.Now.Date;
        var result = new List<Invoice>();
        
        // Get invoices that are due soon (for before-due reminders)
        var dueInvoices = await _invoiceRepository.GetInvoicesDueSoonAsync(daysBefore);
        foreach (var invoice in dueInvoices)
        {
            if (!invoice.IsPaid() && 
                (today.AddDays(daysBefore) >= invoice.DueDate) &&
                !await HasReminderBeenSentAsync(invoice.Id, ReminderType.BeforeDue))
            {
                result.Add(invoice);
            }
        }
        
        // Get invoices due today
        var allInvoices = await _invoiceRepository.GetAllAsync();
        var dueTodayInvoices = allInvoices.Where(i => i.DueDate.Date == today);
        foreach (var invoice in dueTodayInvoices)
        {
            if (!invoice.IsPaid() && 
                !await HasReminderBeenSentAsync(invoice.Id, ReminderType.OnDueDate))
            {
                result.Add(invoice);
            }
        }
        
        // Get overdue invoices
        var overdueInvoices = await _invoiceRepository.GetOverdueInvoicesAsync();
        foreach (var invoice in overdueInvoices)
        {
            var daysSinceOverdue = (today - invoice.DueDate.Date).Days;
            if (!invoice.IsPaid() && 
                (daysSinceOverdue % daysAfter == 0) &&
                !await HasReminderBeenSentTodayAsync(invoice.Id))
            {
                result.Add(invoice);
            }
        }
        
        return result;
    }

    public async Task SendReminderAsync(int invoiceId, ReminderType reminderType, string message)
    {
        var reminder = new Reminder
        {
            InvoiceId = invoiceId,
            Type = reminderType,
            Message = message,
            SentDate = DateTime.Now
        };
        
        await _reminderRepository.AddAsync(reminder);
    }
    
    private async Task<bool> HasReminderBeenSentAsync(int invoiceId, ReminderType reminderType)
    {
        var reminders = await _reminderRepository.GetAllForInvoiceAsync(invoiceId);
        return reminders.Any(r => r.Type == reminderType);
    }
    
    private async Task<bool> HasReminderBeenSentTodayAsync(int invoiceId)
    {
        var today = DateTime.Now.Date;
        var reminders = await _reminderRepository.GetAllForInvoiceAsync(invoiceId);
        return reminders.Any(r => r.SentDate.Date == today);
    }
} 