using InvoiceSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSystem.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Reminder> Reminders { get; set; }
    public DbSet<LateFee> LateFees { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Invoice configuration
        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Payments)
            .WithOne(p => p.Invoice)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Invoice>()
            .HasMany(i => i.Reminders)
            .WithOne(r => r.Invoice)
            .HasForeignKey(r => r.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Payment configuration
        modelBuilder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasColumnType("decimal(18,2)");
        
        // LateFee configuration
        modelBuilder.Entity<LateFee>()
            .Property(lf => lf.Amount)
            .HasColumnType("decimal(18,2)");
    }
} 