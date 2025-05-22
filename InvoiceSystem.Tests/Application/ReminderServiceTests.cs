using InvoiceSystem.Application.Services;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace InvoiceSystem.Tests.Application;

public class ReminderServiceTests
{
    private readonly Mock<IReminderRepository> _mockReminderRepository;
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepository;
    private readonly ReminderService _reminderService;
    
    public ReminderServiceTests()
    {
        _mockReminderRepository = new Mock<IReminderRepository>();
        _mockInvoiceRepository = new Mock<IInvoiceRepository>();
        _reminderService = new ReminderService(_mockReminderRepository.Object, _mockInvoiceRepository.Object);
    }
    
    [Fact]
    [DisplayName("Get Reminders For Invoice - Returns Correct Count")]
    public async Task GetRemindersForInvoice_WhenInvoiceHasReminders_ReturnsCorrectCount()
    {
        // Arrange
        var reminders = new List<Reminder>
        {
            new Reminder { Id = 1, InvoiceId = 1, Type = ReminderType.BeforeDue },
            new Reminder { Id = 2, InvoiceId = 1, Type = ReminderType.OnDueDate }
        };
        
        _mockReminderRepository.Setup(repo => repo.GetAllForInvoiceAsync(1))
            .ReturnsAsync(reminders);
        
        // Act
        var result = await _reminderService.GetRemindersForInvoiceAsync(1);
        
        // Assert
        Assert.Equal(2, result.Count());
    }
    
    [Theory]
    [DisplayName("Get Reminder By ID - Returns Reminder When Exists")]
    [InlineData(1, ReminderType.BeforeDue, true)]
    public async Task GetReminderById_WhenReminderExists_ReturnsReminder(
        int reminderId,
        ReminderType reminderType,
        bool shouldExist)
    {
        // Arrange
        Reminder? setupReminder = new Reminder { Id = reminderId, InvoiceId = 1, Type = reminderType };
            
        _mockReminderRepository.Setup(repo => repo.GetByIdAsync(reminderId))
            .ReturnsAsync(setupReminder);
        
        // Act
        var result = await _reminderService.GetReminderByIdAsync(reminderId);
        
        // Assert
        Assert.NotNull(result);
    }
    
    [Theory]
    [DisplayName("Get Reminder By ID - Returns Reminder With Correct Type")]
    [InlineData(1, ReminderType.BeforeDue, true)]
    public async Task GetReminderById_WhenReminderExists_ReturnsReminderWithCorrectType(
        int reminderId,
        ReminderType reminderType,
        bool shouldExist)
    {
        // Arrange
        Reminder? setupReminder = new Reminder { Id = reminderId, InvoiceId = 1, Type = reminderType };
            
        _mockReminderRepository.Setup(repo => repo.GetByIdAsync(reminderId))
            .ReturnsAsync(setupReminder);
        
        // Act
        var result = await _reminderService.GetReminderByIdAsync(reminderId);
        
        // Assert
        Assert.Equal(reminderType, result!.Type);
    }
    
    [Theory]
    [DisplayName("Get Reminder By ID - Returns Null When Reminder Doesn't Exist")]
    [InlineData(999, ReminderType.BeforeDue, false)]
    public async Task GetReminderById_WhenReminderDoesNotExist_ReturnsNull(
        int reminderId,
        ReminderType reminderType,
        bool shouldExist)
    {
        // Arrange
        _mockReminderRepository.Setup(repo => repo.GetByIdAsync(reminderId))
            .ReturnsAsync((Reminder?)null);
        
        // Act
        var result = await _reminderService.GetReminderByIdAsync(reminderId);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    [DisplayName("Create Reminder - Sets Sent Date")]
    public async Task CreateReminder_WhenCreatingNewReminder_SetsSentDate()
    {
        // Arrange
        var reminder = new Reminder 
        { 
            Id = 0, 
            InvoiceId = 1, 
            Type = ReminderType.BeforeDue,
            Message = "Payment due soon"
        };
        
        _mockReminderRepository.Setup(repo => repo.AddAsync(It.IsAny<Reminder>()))
            .ReturnsAsync((Reminder r) => 
            { 
                r.Id = 1;
                return r; 
            });
        
        // Act
        var result = await _reminderService.CreateReminderAsync(reminder);
        
        // Assert
        Assert.Equal(DateTime.Now.Date, result.SentDate.Date);
    }
    
    [Fact]
    [DisplayName("Create Reminder - Assigns Reminder ID")]
    public async Task CreateReminder_WhenCreatingNewReminder_AssignsReminderId()
    {
        // Arrange
        var reminder = new Reminder 
        { 
            Id = 0, 
            InvoiceId = 1, 
            Type = ReminderType.BeforeDue,
            Message = "Payment due soon"
        };
        
        _mockReminderRepository.Setup(repo => repo.AddAsync(It.IsAny<Reminder>()))
            .ReturnsAsync((Reminder r) => 
            { 
                r.Id = 1;
                return r; 
            });
        
        // Act
        var result = await _reminderService.CreateReminderAsync(reminder);
        
        // Assert
        Assert.Equal(1, result.Id);
    }
    
    [Fact]
    [DisplayName("Send Reminder - Creates Reminder With Correct Properties")]
    public async Task SendReminder_WhenSendingReminder_CreatesReminderWithCorrectProperties()
    {
        // Arrange
        int invoiceId = 1;
        ReminderType reminderType = ReminderType.OnDueDate;
        string message = "Your payment is due today";
        
        Reminder capturedReminder = null;
        
        _mockReminderRepository.Setup(repo => repo.AddAsync(It.IsAny<Reminder>()))
            .Callback<Reminder>(r => capturedReminder = r)
            .ReturnsAsync((Reminder r) => 
            { 
                r.Id = 1;
                return r; 
            });
        
        // Act
        await _reminderService.SendReminderAsync(invoiceId, reminderType, message);
        
        // Assert
        Assert.NotNull(capturedReminder);
        Assert.Equal(invoiceId, capturedReminder.InvoiceId);
        Assert.Equal(reminderType, capturedReminder.Type);
        Assert.Equal(message, capturedReminder.Message);
        Assert.Equal(DateTime.Now.Date, capturedReminder.SentDate.Date);
    }
    
    [Fact]
    [DisplayName("Update Reminder - Calls Repository Update")]
    public async Task UpdateReminder_WhenUpdatingReminder_CallsUpdateMethod()
    {
        // Arrange
        var reminder = new Reminder { Id = 1, InvoiceId = 1, Type = ReminderType.BeforeDue };
        
        _mockReminderRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Reminder>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await _reminderService.UpdateReminderAsync(reminder);
        
        // Assert
        _mockReminderRepository.Verify(repo => repo.UpdateAsync(reminder), Times.Once);
    }
    
    [Fact]
    [DisplayName("Get Invoices Needing Reminders - Identifies Invoices Due Soon")]
    public async Task GetInvoicesNeedingReminders_WhenInvoicesDueSoon_ReturnsCorrectInvoices()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var invoiceDueSoon = new Invoice 
        { 
            Id = 1, 
            Amount = 100.0m, 
            DueDate = today.AddDays(5),
            Status = InvoiceStatus.Pending,
            Payments = new List<Payment>()
        };
        
        var dueInvoices = new List<Invoice> { invoiceDueSoon };
        
        _mockInvoiceRepository.Setup(repo => repo.GetInvoicesDueSoonAsync(10))
            .ReturnsAsync(dueInvoices);
            
        _mockReminderRepository.Setup(repo => repo.GetAllForInvoiceAsync(1))
            .ReturnsAsync(new List<Reminder>());
        
        // Act
        var result = await _reminderService.GetInvoicesNeedingRemindersAsync(10, 7);
        
        // Assert
        Assert.Contains(result, i => i.Id == 1);
    }
    
    [Fact]
    [DisplayName("Get Invoices Needing Reminders - Excludes Paid Invoices")]
    public async Task GetInvoicesNeedingReminders_WhenInvoiceIsPaid_ExcludesFromResults()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var invoiceDueSoon = new Invoice 
        { 
            Id = 1, 
            Amount = 100.0m, 
            DueDate = today.AddDays(5),
            Status = InvoiceStatus.Paid,
            Payments = new List<Payment> { new Payment { Amount = 100.0m } }
        };
        
        var dueInvoices = new List<Invoice> { invoiceDueSoon };
        
        _mockInvoiceRepository.Setup(repo => repo.GetInvoicesDueSoonAsync(10))
            .ReturnsAsync(dueInvoices);
        
        // Act
        var result = await _reminderService.GetInvoicesNeedingRemindersAsync(10, 7);
        
        // Assert
        Assert.DoesNotContain(result, i => i.Id == 1);
    }
    
    [Fact]
    [DisplayName("Get Invoices Needing Reminders - Excludes Already Reminded Invoices")]
    public async Task GetInvoicesNeedingReminders_WhenReminderAlreadySent_ExcludesFromResults()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var invoiceDueSoon = new Invoice 
        { 
            Id = 1, 
            Amount = 100.0m, 
            DueDate = today.AddDays(5),
            Status = InvoiceStatus.Pending,
            Payments = new List<Payment>()
        };
        
        var dueInvoices = new List<Invoice> { invoiceDueSoon };
        var existingReminders = new List<Reminder> 
        { 
            new Reminder 
            { 
                Id = 1, 
                InvoiceId = 1, 
                Type = ReminderType.BeforeDue,
                SentDate = today.AddDays(-1)
            } 
        };
        
        _mockInvoiceRepository.Setup(repo => repo.GetInvoicesDueSoonAsync(10))
            .ReturnsAsync(dueInvoices);
            
        _mockReminderRepository.Setup(repo => repo.GetAllForInvoiceAsync(1))
            .ReturnsAsync(existingReminders);
        
        // Act
        var result = await _reminderService.GetInvoicesNeedingRemindersAsync(10, 7);
        
        // Assert
        Assert.DoesNotContain(result, i => i.Id == 1);
    }
    
    [Fact]
    [DisplayName("Get Invoices Needing Reminders - Identifies Overdue Invoices")]
    public async Task GetInvoicesNeedingReminders_WhenInvoicesOverdue_ReturnsCorrectInvoices()
    {
        // Arrange
        var today = DateTime.Now.Date;
        var overdueInvoice = new Invoice 
        { 
            Id = 2, 
            Amount = 100.0m, 
            DueDate = today.AddDays(-7),
            Status = InvoiceStatus.Overdue,
            Payments = new List<Payment>()
        };
        
        var overdueInvoices = new List<Invoice> { overdueInvoice };
        
        _mockInvoiceRepository.Setup(repo => repo.GetOverdueInvoicesAsync())
            .ReturnsAsync(overdueInvoices);
            
        _mockReminderRepository.Setup(repo => repo.GetAllForInvoiceAsync(2))
            .ReturnsAsync(new List<Reminder>());
            
        _mockInvoiceRepository.Setup(repo => repo.GetInvoicesDueSoonAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Invoice>());
            
        _mockInvoiceRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<Invoice>());
        
        // Act
        var result = await _reminderService.GetInvoicesNeedingRemindersAsync(10, 7);
        
        // Assert
        Assert.Contains(result, i => i.Id == 2);
    }
} 