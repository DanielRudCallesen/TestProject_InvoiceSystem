namespace InvoiceSystem.Domain.Entities;

public class LateFee
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public DateTime AppliedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public Invoice? Invoice { get; set; }
} 