namespace InvoiceSystem.Domain.Entities;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public InvoiceStatus Status { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public List<Payment> Payments { get; set; } = new List<Payment>();
    public List<Reminder> Reminders { get; set; } = new List<Reminder>();
    
    public decimal GetRemainingAmount()
    {
        decimal totalPaid = Payments.Sum(p => p.Amount);
        return Amount - totalPaid;
    }
    
    public bool IsPaid()
    {
        return GetRemainingAmount() <= 0;
    }
    
    public bool IsOverdue()
    {
        return DueDate < DateTime.Now && !IsPaid();
    }
    
    public void UpdateStatus()
    {
        if (IsPaid())
        {
            Status = InvoiceStatus.Paid;
        }
        else if (IsOverdue())
        {
            Status = InvoiceStatus.Overdue;
        }
        else
        {
            Status = InvoiceStatus.Pending;
        }
    }
}

public enum InvoiceStatus
{
    Pending,
    Paid,
    Overdue,
    Cancelled
} 