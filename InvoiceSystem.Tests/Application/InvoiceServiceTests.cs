using InvoiceSystem.Application.Services;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using Moq;
using System.Collections.Generic;
using System.ComponentModel;
using Xunit;

namespace InvoiceSystem.Tests.Application;

public class InvoiceServiceTests
{
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepository;
    private readonly InvoiceService _invoiceService;
    
    public InvoiceServiceTests()
    {
        _mockInvoiceRepository = new Mock<IInvoiceRepository>();
        _invoiceService = new InvoiceService(_mockInvoiceRepository.Object);
    }
    
    public static IEnumerable<object[]> InvoiceListTestData()
    {
        var invoices = new List<Invoice>
        {
            new Invoice { Id = 1, InvoiceNumber = "INV-001" },
            new Invoice { Id = 2, InvoiceNumber = "INV-002" }
        };
        
        yield return new object[] { invoices, 2 };
    }
    
    [Theory]
    [DisplayName("Get All Invoices - Returns Correct Count")]
    [MemberData(nameof(InvoiceListTestData))]
    public async Task GetAllInvoices_WhenInvoicesExist_ReturnsCorrectCount(List<Invoice> invoices, int expectedCount)
    {
        // Arrange
        _mockInvoiceRepository.Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(invoices);
        
        // Act
        var result = await _invoiceService.GetAllInvoicesAsync();
        
        // Assert
        Assert.Equal(expectedCount, result.Count());
    }
    
    [Theory]
    [DisplayName("Get Invoice By ID - When Invoice Exists - Returns Invoice")]
    [InlineData(1, "INV-001", true)]    // Valid ID
    public async Task GetInvoiceById_WhenInvoiceExists_ReturnsInvoice(
        int invoiceId, 
        string expectedInvoiceNumber, 
        bool shouldExist)
    {
        // Arrange
        Invoice? setupInvoice = new Invoice { Id = invoiceId, InvoiceNumber = expectedInvoiceNumber };
            
        _mockInvoiceRepository.Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync(setupInvoice);
        
        // Act
        var result = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        
        // Assert
        Assert.NotNull(result);
    }
    
    [Theory]
    [DisplayName("Get Invoice By ID - When Invoice Exists - Has Correct Invoice Number")]
    [InlineData(1, "INV-001", true)]    // Valid ID
    public async Task GetInvoiceById_WhenInvoiceExists_HasCorrectInvoiceNumber(
        int invoiceId, 
        string expectedInvoiceNumber, 
        bool shouldExist)
    {
        // Arrange
        Invoice? setupInvoice = new Invoice { Id = invoiceId, InvoiceNumber = expectedInvoiceNumber };
            
        _mockInvoiceRepository.Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync(setupInvoice);
        
        // Act
        var result = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        
        // Assert
        Assert.Equal(expectedInvoiceNumber, result!.InvoiceNumber);
    }
    
    [Theory]
    [DisplayName("Get Invoice By ID - When Invoice Doesn't Exist - Returns Null")]
    [InlineData(999, null, false)]      // Invalid ID
    public async Task GetInvoiceById_WhenInvoiceDoesNotExist_ReturnsNull(
        int invoiceId, 
        string? expectedInvoiceNumber, 
        bool shouldExist)
    {
        // Arrange            
        _mockInvoiceRepository.Setup(repo => repo.GetByIdAsync(invoiceId))
            .ReturnsAsync((Invoice?)null);
        
        // Act
        var result = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    [DisplayName("Create Invoice - Sets Correct Created Date")]
    public async Task CreateInvoice_WhenCreatingNewInvoice_SetsCorrectCreatedDate()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-001",
            Amount = 100.0m,
            CustomerName = "Test Customer"
        };
        
        _mockInvoiceRepository.Setup(repo => repo.AddAsync(It.IsAny<Invoice>()))
            .ReturnsAsync((Invoice inv) => inv);
        
        // Act
        var result = await _invoiceService.CreateInvoiceAsync(invoice);
        
        // Assert
        Assert.Equal(DateTime.Now.Date, result.CreatedDate.Date);
    }
    
    [Fact]
    [DisplayName("Create Invoice - Sets Pending Status")]
    public async Task CreateInvoice_WhenCreatingNewInvoice_SetsPendingStatus()
    {
        // Arrange
        var invoice = new Invoice
        {
            InvoiceNumber = "INV-001",
            Amount = 100.0m,
            CustomerName = "Test Customer"
        };
        
        _mockInvoiceRepository.Setup(repo => repo.AddAsync(It.IsAny<Invoice>()))
            .ReturnsAsync((Invoice inv) => inv);
        
        // Act
        var result = await _invoiceService.CreateInvoiceAsync(invoice);
        
        // Assert
        Assert.Equal(InvoiceStatus.Pending, result.Status);
    }
    
    public static IEnumerable<object[]> InvoiceStatusTestData()
    {
        // Structure: days offset for due date, payment amount, initial status, expected status after update
        yield return new object[] { -5, 0.0m, InvoiceStatus.Pending, InvoiceStatus.Overdue };  // Overdue, no payment
        yield return new object[] { 5, 0.0m, InvoiceStatus.Pending, InvoiceStatus.Pending };   // Not overdue, no payment
        yield return new object[] { -5, 100.0m, InvoiceStatus.Overdue, InvoiceStatus.Paid };   // Overdue but paid
        yield return new object[] { 5, 100.0m, InvoiceStatus.Pending, InvoiceStatus.Paid };    // Not overdue, fully paid
    }
    
    [Theory]
    [DisplayName("Update Invoice - Updates Status Correctly Based On Conditions")]
    [MemberData(nameof(InvoiceStatusTestData))]
    public async Task UpdateInvoice_WhenConditionsChange_UpdatesStatusCorrectly(
        int dueDateOffsetDays, 
        decimal paymentAmount, 
        InvoiceStatus initialStatus, 
        InvoiceStatus expectedStatus)
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            InvoiceNumber = "INV-001",
            Amount = 100.0m,
            Status = initialStatus,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = paymentAmount > 0 
                ? new List<Payment> { new Payment { Amount = paymentAmount } }
                : new List<Payment>()
        };
        
        _mockInvoiceRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await _invoiceService.UpdateInvoiceAsync(invoice);
        
        // Assert
        Assert.Equal(expectedStatus, invoice.Status);
    }
    
    [Theory]
    [DisplayName("Calculate Remaining Amount - Returns Correct Amount")]
    [InlineData(100.0, 0.0, 100.0)]    // No payments
    [InlineData(100.0, 30.0, 70.0)]    // Partial payment
    [InlineData(100.0, 100.0, 0.0)]    // Full payment
    public async Task CalculateRemainingAmount_WithDifferentPaymentAmounts_ReturnsCorrectAmount(
        decimal invoiceAmount, 
        decimal paymentAmount, 
        decimal expectedRemaining)
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            Amount = invoiceAmount,
            Payments = paymentAmount > 0 
                ? new List<Payment> { new Payment { Amount = paymentAmount } }
                : new List<Payment>()
        };
        
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
        
        // Act
        var result = await _invoiceService.CalculateRemainingAmountAsync(1);
        
        // Assert
        Assert.Equal(expectedRemaining, result);
    }
    
    [Theory]
    [InlineData(0.0, false)]      // No payment
    [InlineData(0.1, false)]      // Partial payment
    [InlineData(50.0, false)]    // Partial payment
    [InlineData(99.99, false)]   // Partial payment
    public async Task IsInvoicePaid_WhenInvoiceNotFullyPaid_ReturnsFalse(
        decimal paymentAmount, 
        bool expectedIsPaid)
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            Amount = 100.0m,
            Payments = paymentAmount > 0 
                ? new List<Payment> { new Payment { Amount = paymentAmount } }
                : new List<Payment>()
        };
        
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
        
        // Act
        var result = await _invoiceService.IsInvoicePaidAsync(1);
        
        // Assert
        Assert.Equal(expectedIsPaid, result);
    }
    
    [Theory]
    [DisplayName("Is Invoice Paid - Returns True When Fully Paid")]
    [InlineData(100.0, true)]     // Full payment
    [InlineData(120.0, true)]     // Overpayment
    public async Task IsInvoicePaid_WhenInvoiceFullyPaid_ReturnsTrue(
        decimal paymentAmount, 
        bool expectedIsPaid)
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            Amount = 100.0m,
            Payments = new List<Payment> { new Payment { Amount = paymentAmount } }
        };
        
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
        
        // Act
        var result = await _invoiceService.IsInvoicePaidAsync(1);
        
        // Assert
        Assert.Equal(expectedIsPaid, result);
    }
    
    [Theory]
    [DisplayName("Is Invoice Overdue - Returns False When Due Date Is In Future")]
    [InlineData(5, false)]     // Due date in future
    public async Task IsInvoiceOverdue_WhenDueDateInFuture_ReturnsFalse(
        int dueDateOffsetDays, 
        bool expectedIsOverdue)
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            Amount = 100.0m,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment>()
        };
        
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
        
        // Act
        var result = await _invoiceService.IsInvoiceOverdueAsync(1);
        
        // Assert
        Assert.Equal(expectedIsOverdue, result);
    }
    
    [Theory]
    [DisplayName("Is Invoice Overdue - Returns True When Due Date Has Passed")]
    [InlineData(-5, true)]     // Due date passed
    public async Task IsInvoiceOverdue_WhenDueDatePassed_ReturnsTrue(
        int dueDateOffsetDays, 
        bool expectedIsOverdue)
    {
        // Arrange
        var invoice = new Invoice
        {
            Id = 1,
            Amount = 100.0m,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment>()
        };
        
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
        
        // Act
        var result = await _invoiceService.IsInvoiceOverdueAsync(1);
        
        // Assert
        Assert.Equal(expectedIsOverdue, result);
    }
}