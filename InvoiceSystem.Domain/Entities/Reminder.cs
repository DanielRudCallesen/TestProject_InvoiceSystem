namespace InvoiceSystem.Domain.Entities;

public class Reminder
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public DateTime SentDate { get; set; }
    public ReminderType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    
    public Invoice? Invoice { get; set; }
}

public enum ReminderType
{
    BeforeDue,
    OnDueDate,
    AfterDue
} 