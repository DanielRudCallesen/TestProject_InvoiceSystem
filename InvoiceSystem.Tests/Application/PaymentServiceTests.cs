using InvoiceSystem.Application.Services;
using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Repositories;
using Moq;
using System.Collections.Generic;
using System.ComponentModel;

using Xunit;

namespace InvoiceSystem.Tests.Application;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _mockPaymentRepository;
    private readonly Mock<IInvoiceRepository> _mockInvoiceRepository;
    private readonly PaymentService _paymentService;
    
    public PaymentServiceTests()
    {
        _mockPaymentRepository = new Mock<IPaymentRepository>();
        _mockInvoiceRepository = new Mock<IInvoiceRepository>();
        _paymentService = new PaymentService(_mockPaymentRepository.Object, _mockInvoiceRepository.Object);
    }
    
    [Fact]
    [DisplayName("Get Payments For Invoice - Returns Correct Count")]
    public async Task GetPaymentsForInvoice_WhenInvoiceHasPayments_ReturnsCorrectCount()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new Payment { Id = 1, InvoiceId = 1, Amount = 50.0m },
            new Payment { Id = 2, InvoiceId = 1, Amount = 25.0m }
        };
        
        _mockPaymentRepository.Setup(repo => repo.GetAllForInvoiceAsync(1))
            .ReturnsAsync(payments);
        
        // Act
        var result = await _paymentService.GetPaymentsForInvoiceAsync(1);
        
        // Assert
        Assert.Equal(2, result.Count());
    }
    
    [Fact]
    [DisplayName("Get Payments For Invoice - First Payment Has Correct Amount")]
    public async Task GetPaymentsForInvoice_WhenInvoiceHasPayments_ReturnsFirstPaymentWithCorrectAmount()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new Payment { Id = 1, InvoiceId = 1, Amount = 50.0m },
            new Payment { Id = 2, InvoiceId = 1, Amount = 25.0m }
        };
        
        _mockPaymentRepository.Setup(repo => repo.GetAllForInvoiceAsync(1))
            .ReturnsAsync(payments);
        
        // Act
        var result = await _paymentService.GetPaymentsForInvoiceAsync(1);
        
        // Assert
        Assert.Equal(50.0m, result.First().Amount);
    }
    
    [Fact]
    [DisplayName("Get Payments For Invoice - Last Payment Has Correct Amount")]
    public async Task GetPaymentsForInvoice_WhenInvoiceHasPayments_ReturnsLastPaymentWithCorrectAmount()
    {
        // Arrange
        var payments = new List<Payment>
        {
            new Payment { Id = 1, InvoiceId = 1, Amount = 50.0m },
            new Payment { Id = 2, InvoiceId = 1, Amount = 25.0m }
        };
        
        _mockPaymentRepository.Setup(repo => repo.GetAllForInvoiceAsync(1))
            .ReturnsAsync(payments);
        
        // Act
        var result = await _paymentService.GetPaymentsForInvoiceAsync(1);
        
        // Assert
        Assert.Equal(25.0m, result.Last().Amount);
    }
    
    [Theory]
    [DisplayName("Get Payment By ID - Returns Payment When Exists")]
    [InlineData(1, 50.0, true)]    // Valid payment ID
    public async Task GetPaymentById_WhenPaymentExists_ReturnsPayment(
        int paymentId, 
        decimal paymentAmount, 
        bool shouldExist)
    {
        // Arrange
        Payment? setupPayment = new Payment { Id = paymentId, InvoiceId = 1, Amount = paymentAmount };
            
        _mockPaymentRepository.Setup(repo => repo.GetByIdAsync(paymentId))
            .ReturnsAsync(setupPayment);
        
        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);
        
        // Assert
        Assert.NotNull(result);
    }
    
    [Theory]
    [DisplayName("Get Payment By ID - Returns Payment With Correct Amount")]
    [InlineData(1, 50.0, true)]    // Valid payment ID
    public async Task GetPaymentById_WhenPaymentExists_ReturnsPaymentWithCorrectAmount(
        int paymentId, 
        decimal paymentAmount, 
        bool shouldExist)
    {
        // Arrange
        Payment? setupPayment = new Payment { Id = paymentId, InvoiceId = 1, Amount = paymentAmount };
            
        _mockPaymentRepository.Setup(repo => repo.GetByIdAsync(paymentId))
            .ReturnsAsync(setupPayment);
        
        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);
        
        // Assert
        Assert.Equal(paymentAmount, result!.Amount);
    }
    
    [Theory]
    [DisplayName("Get Payment By ID - Returns Null When Payment Doesn't Exist")]
    [InlineData(999, 0.0, false)]  // Invalid payment ID
    public async Task GetPaymentById_WhenPaymentDoesNotExist_ReturnsNull(
        int paymentId, 
        decimal paymentAmount, 
        bool shouldExist)
    {
        // Arrange
        _mockPaymentRepository.Setup(repo => repo.GetByIdAsync(paymentId))
            .ReturnsAsync((Payment?)null);
        
        // Act
        var result = await _paymentService.GetPaymentByIdAsync(paymentId);
        
        // Assert
        Assert.Null(result);
    }
    
    public static IEnumerable<object[]> PaymentScenarioData()
    {
        // Payment amount, Invoice amount, Due date offset, Expected status
        yield return new object[] { 50.0m, 100.0m, 10, InvoiceStatus.Pending };   // Partial payment, not overdue
        yield return new object[] { 100.0m, 100.0m, 10, InvoiceStatus.Paid };     // Full payment, not overdue
        yield return new object[] { 120.0m, 100.0m, 10, InvoiceStatus.Paid };     // Overpayment, not overdue
        yield return new object[] { 50.0m, 100.0m, -5, InvoiceStatus.Overdue };   // Partial payment, overdue
        yield return new object[] { 100.0m, 100.0m, -5, InvoiceStatus.Paid };     // Full payment, was overdue
    }
    
    [Theory]
    [DisplayName("Record Payment - Assigns Payment ID")]
    [MemberData(nameof(PaymentScenarioData))]
    public async Task RecordPayment_WhenNewPaymentRecorded_AssignsPaymentId(
        decimal paymentAmount, 
        decimal invoiceAmount, 
        int dueDateOffsetDays, 
        InvoiceStatus expectedStatus)
    {
        // Arrange
        var payment = new Payment { Id = 0, InvoiceId = 1, Amount = paymentAmount };
        var invoice = new Invoice 
        { 
            Id = 1, 
            Amount = invoiceAmount, 
            Status = InvoiceStatus.Pending,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment>()
        };
        
        _mockPaymentRepository.Setup(repo => repo.AddAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => 
            { 
                p.Id = 1; 
                invoice.Payments.Add(p);
                return p; 
            });
            
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
            
        _mockInvoiceRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _paymentService.RecordPaymentAsync(payment);
        
        // Assert
        Assert.Equal(1, result.Id);
    }
    
    [Theory]
    [DisplayName("Record Payment - Sets Payment Date")]
    [MemberData(nameof(PaymentScenarioData))]
    public async Task RecordPayment_WhenNewPaymentRecorded_SetsPaymentDate(
        decimal paymentAmount, 
        decimal invoiceAmount, 
        int dueDateOffsetDays, 
        InvoiceStatus expectedStatus)
    {
        // Arrange
        var payment = new Payment { Id = 0, InvoiceId = 1, Amount = paymentAmount };
        var invoice = new Invoice 
        { 
            Id = 1, 
            Amount = invoiceAmount, 
            Status = InvoiceStatus.Pending,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment>()
        };
        
        _mockPaymentRepository.Setup(repo => repo.AddAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => 
            { 
                p.Id = 1; 
                invoice.Payments.Add(p);
                return p; 
            });
            
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
            
        _mockInvoiceRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);
        
        // Act
        var result = await _paymentService.RecordPaymentAsync(payment);
        
        // Assert
        Assert.Equal(DateTime.Now.Date, result.PaymentDate.Date);
    }
    
    [Theory]
    [DisplayName("Record Payment - Updates Invoice Status Correctly")]
    [MemberData(nameof(PaymentScenarioData))]
    public async Task RecordPayment_WhenPaymentUpdatesInvoiceStatus_SetsCorrectStatus(
        decimal paymentAmount, 
        decimal invoiceAmount, 
        int dueDateOffsetDays, 
        InvoiceStatus expectedStatus)
    {
        // Arrange
        var payment = new Payment { Id = 0, InvoiceId = 1, Amount = paymentAmount };
        var invoice = new Invoice 
        { 
            Id = 1, 
            Amount = invoiceAmount, 
            Status = InvoiceStatus.Pending,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment>()
        };
        
        _mockPaymentRepository.Setup(repo => repo.AddAsync(It.IsAny<Payment>()))
            .ReturnsAsync((Payment p) => 
            { 
                p.Id = 1; 
                invoice.Payments.Add(p);
                return p; 
            });
            
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
            
        _mockInvoiceRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentService.RecordPaymentAsync(payment);
        
        // Assert
        Assert.Equal(expectedStatus, invoice.Status);
    }
    
    [Theory]
    [DisplayName("Update Payment - Updates Invoice Status")]
    [MemberData(nameof(PaymentScenarioData))]
    public async Task UpdatePayment_WhenPaymentUpdated_UpdatesInvoiceStatus(
        decimal paymentAmount, 
        decimal invoiceAmount, 
        int dueDateOffsetDays, 
        InvoiceStatus expectedStatus)
    {
        // Arrange
        var payment = new Payment { Id = 1, InvoiceId = 1, Amount = paymentAmount };
        var invoice = new Invoice 
        { 
            Id = 1, 
            Amount = invoiceAmount, 
            Status = InvoiceStatus.Pending,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment> { payment }  // Include the payment in the invoice
        };
        
        _mockPaymentRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Payment>()))
            .Returns(Task.CompletedTask);
            
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
            
        _mockInvoiceRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentService.UpdatePaymentAsync(payment);
        
        // Assert
        Assert.Equal(expectedStatus, invoice.Status);
    }
    
    [Theory]
    [DisplayName("Delete Payment - Updates Invoice Status")]
    [InlineData(true, InvoiceStatus.Pending)]
    public async Task DeletePayment_WhenPaymentExists_UpdatesInvoiceStatus(
        bool paymentExists, 
        InvoiceStatus expectedStatus)
    {
        // Arrange
        var payment = new Payment { Id = 1, InvoiceId = 1, Amount = 50.0m };
        var invoice = new Invoice 
        { 
            Id = 1, 
            Amount = 100.0m, 
            Status = InvoiceStatus.Pending,
            DueDate = DateTime.Now.AddDays(10),
            Payments = new List<Payment>()
        };
        
        _mockPaymentRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(payment);
            
        _mockPaymentRepository.Setup(repo => repo.DeleteAsync(1))
            .Returns(Task.CompletedTask);
                
        _mockInvoiceRepository.Setup(repo => repo.GetByIdWithDetailsAsync(1))
            .ReturnsAsync(invoice);
                
        _mockInvoiceRepository.Setup(repo => repo.UpdateAsync(It.IsAny<Invoice>()))
            .Returns(Task.CompletedTask);
        
        // Act
        await _paymentService.DeletePaymentAsync(1);
        
        // Assert
        Assert.Equal(expectedStatus, invoice.Status);
    }
    
    [Theory]
    [DisplayName("Delete Payment - Doesn't Call Delete When Payment Doesn't Exist")]
    [InlineData(false)]
    public async Task DeletePayment_WhenPaymentDoesNotExist_DoesNotCallDeleteMethod(bool paymentExists)
    {
        // Arrange
        _mockPaymentRepository.Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync((Payment?)null);
        
        // Act
        await _paymentService.DeletePaymentAsync(1);
        
        // Assert
        _mockPaymentRepository.Verify(repo => repo.DeleteAsync(It.IsAny<int>()), Times.Never);
    }
    
    [Theory]
    [DisplayName("Get Total Payments - Returns Sum Of All Payments")]
    [InlineData(new double[] { 40.0, 30.0 }, 70.0)]        // Multiple payments
    [InlineData(new double[] { 100.0 }, 100.0)]            // Single payment
    [InlineData(new double[] {}, 0.0)]                     // No payments
    public async Task GetTotalPaymentsForInvoice_WithDifferentPaymentScenarios_ReturnsSumOfPayments(
        double[] paymentAmounts, 
        decimal expectedTotal)
    {
        // Arrange
        var payments = new List<Payment>();
        int id = 1;
        
        foreach (var amount in paymentAmounts)
        {
            payments.Add(new Payment { Id = id++, InvoiceId = 1, Amount = (decimal)amount });
        }
        
        _mockPaymentRepository.Setup(repo => repo.GetAllForInvoiceAsync(1))
            .ReturnsAsync(payments);
        
        // Act
        var result = await _paymentService.GetTotalPaymentsForInvoiceAsync(1);
        
        // Assert
        Assert.Equal(expectedTotal, result);
    }
} 