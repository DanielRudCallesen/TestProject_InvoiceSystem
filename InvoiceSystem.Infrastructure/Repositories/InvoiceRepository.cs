using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using InvoiceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSystem.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly ApplicationDbContext _context;
    
    public InvoiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices.ToListAsync();
    }
    
    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices.FindAsync(id);
    }
    
    public async Task<Invoice?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Invoices
            .Include(i => i.Payments)
            .Include(i => i.Reminders)
            .FirstOrDefaultAsync(i => i.Id == id);
    }
    
    public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
    {
        var today = DateTime.Now.Date;
        return await _context.Invoices
            .Include(i => i.Payments)
            .Where(i => i.DueDate.Date < today && i.Status != InvoiceStatus.Paid)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<Invoice>> GetInvoicesDueSoonAsync(int daysThreshold)
    {
        var today = DateTime.Now.Date;
        var threshold = today.AddDays(daysThreshold);
        return await _context.Invoices
            .Include(i => i.Payments)
            .Where(i => i.DueDate.Date <= threshold && i.DueDate.Date >= today && i.Status != InvoiceStatus.Paid)
            .ToListAsync();
    }
    
    public async Task<Invoice> AddAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }
    
    public async Task UpdateAsync(Invoice invoice)
    {
        // Get the tracked entity
    var existingInvoice = await _context.Invoices.FindAsync(invoice.Id);
    if (existingInvoice != null)
    {
        // Transfer properties
        _context.Entry(existingInvoice).CurrentValues.SetValues(invoice);
        await _context.SaveChangesAsync();
    }
    }
    
    public async Task DeleteAsync(int id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice != null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }
    }
} 