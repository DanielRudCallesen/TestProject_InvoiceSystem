using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using InvoiceSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSystem.Infrastructure.Repositories;

public class LateFeeRepository : ILateFeeRepository
{
    private readonly ApplicationDbContext _context;
    
    public LateFeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<LateFee>> GetAllForInvoiceAsync(int invoiceId)
    {
        return await _context.LateFees
            .Where(lf => lf.InvoiceId == invoiceId)
            .ToListAsync();
    }
    
    public async Task<LateFee?> GetByIdAsync(int id)
    {
        return await _context.LateFees.FindAsync(id);
    }
    
    public async Task<LateFee> AddAsync(LateFee lateFee)
    {
        _context.LateFees.Add(lateFee);
        await _context.SaveChangesAsync();
        return lateFee;
    }
    
    public async Task UpdateAsync(LateFee lateFee)
    {
        _context.Entry(lateFee).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteAsync(int id)
    {
        var lateFee = await _context.LateFees.FindAsync(id);
        if (lateFee != null)
        {
            _context.LateFees.Remove(lateFee);
            await _context.SaveChangesAsync();
        }
    }
} 