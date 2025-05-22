using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using InvoiceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSystem.Infrastructure.Repositories;

public class ReminderRepository : IReminderRepository
{
    private readonly ApplicationDbContext _context;
    
    public ReminderRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Reminder>> GetAllForInvoiceAsync(int invoiceId)
    {
        return await _context.Reminders
            .Where(r => r.InvoiceId == invoiceId)
            .ToListAsync();
    }
    
    public async Task<Reminder?> GetByIdAsync(int id)
    {
        return await _context.Reminders.FindAsync(id);
    }
    
    public async Task<Reminder> AddAsync(Reminder reminder)
    {
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();
        return reminder;
    }
    
    public async Task UpdateAsync(Reminder reminder)
    {
        _context.Entry(reminder).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var reminder = await _context.Reminders.FindAsync(id);
        if (reminder != null)
        {
            _context.Reminders.Remove(reminder);
            await _context.SaveChangesAsync();
        }
    }
} 