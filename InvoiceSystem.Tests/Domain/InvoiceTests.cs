using InvoiceSystem.Domain.Entities;
using System.Collections.Generic;
using Xunit;
using System.ComponentModel;


namespace InvoiceSystem.Tests.Domain;

public class InvoiceTests
{
    [Theory]
    [DisplayName("Remaining Amount - Calculates Correctly With Single Payment")]
    [InlineData(100.0, 0.0, 100.0)]        // No payments
    [InlineData(100.0, 30.0, 70.0)]        // Partial payment
    [InlineData(100.0, 100.0, 0.0)]        // Full payment
    [InlineData(100.0, 120.0, -20.0)]      // Overpayment
    public void GetRemainingAmount_WhenPaymentsExist_ReturnsCorrectAmount(decimal invoiceAmount, decimal paymentAmount, decimal expectedRemaining)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = invoiceAmount,
            Payments = paymentAmount > 0 
                ? new List<Payment> { new Payment { Amount = paymentAmount } }
                : new List<Payment>()
        };
        
        // Act
        var remainingAmount = invoice.GetRemainingAmount();
        
        // Assert
        Assert.Equal(expectedRemaining, remainingAmount);
    }
    
    [Theory]
    [DisplayName("Is Paid - Returns False When Not Fully Paid")]
    [InlineData(100.0, 0.0, false)]      // No payments
    [InlineData(100.0, 50.0, false)]     // Partial payment
    public void IsPaid_WhenInvoiceNotFullyPaid_ReturnsFalse(decimal invoiceAmount, decimal paymentAmount, bool expectedIsPaid)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = invoiceAmount,
            Payments = paymentAmount > 0 
                ? new List<Payment> { new Payment { Amount = paymentAmount } }
                : new List<Payment>()
        };
        
        // Act
        var isPaid = invoice.IsPaid();
        
        // Assert
        Assert.Equal(expectedIsPaid, isPaid);
    }
    
    [Theory]
    [DisplayName("Is Paid - Returns True When Fully Paid")]
    [InlineData(100.0, 100.0, true)]     // Full payment
    [InlineData(100.0, 120.0, true)]     // Overpayment
    public void IsPaid_WhenInvoiceFullyPaid_ReturnsTrue(decimal invoiceAmount, decimal paymentAmount, bool expectedIsPaid)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = invoiceAmount,
            Payments = new List<Payment> { new Payment { Amount = paymentAmount } }
        };
        
        // Act
        var isPaid = invoice.IsPaid();
        
        // Assert
        Assert.Equal(expectedIsPaid, isPaid);
    }
    
    public static IEnumerable<object[]> OverdueTestData()
    {
        yield return new object[] { 5, 0.0, false };   // Due date in future, no payment
        yield return new object[] { -5, 50.0, true };  // Due date passed, partially paid
    }
    
    [Theory]
    [DisplayName("Is Overdue - Evaluates Based On Multiple Conditions")]
    [MemberData(nameof(OverdueTestData))]
    public void IsOverdue_WhenConditionsIndicate_ReturnsExpectedResult(int dueDateOffsetDays, decimal paymentAmount, bool expectedIsOverdue)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = 100.0m,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = paymentAmount > 0 
                ? new List<Payment> { new Payment { Amount = paymentAmount } }
                : new List<Payment>()
        };
        
        // Act
        var isOverdue = invoice.IsOverdue();
        
        // Assert
        Assert.Equal(expectedIsOverdue, isOverdue);
    }
    
    [Theory]
    [DisplayName("Is Overdue - Returns True When Due Date Passed And Not Paid")]
    [InlineData(-5, 0.0, true)]   // Due date passed, no payment
    public void IsOverdue_WhenDueDatePassedAndNotPaid_ReturnsTrue(int dueDateOffsetDays, decimal paymentAmount, bool expectedIsOverdue)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = 100.0m,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment>()
        };
        
        // Act
        var isOverdue = invoice.IsOverdue();
        
        // Assert
        Assert.Equal(expectedIsOverdue, isOverdue);
    }
    
    [Theory]
    [DisplayName("Is Overdue - Returns False When Due Date Passed But Fully Paid")]
    [InlineData(-5, 100.0, false)] // Due date passed, fully paid
    public void IsOverdue_WhenDueDatePassedButFullyPaid_ReturnsFalse(int dueDateOffsetDays, decimal paymentAmount, bool expectedIsOverdue)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = 100.0m,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = new List<Payment> { new Payment { Amount = paymentAmount } }
        };
        
        // Act
        var isOverdue = invoice.IsOverdue();
        
        // Assert
        Assert.Equal(expectedIsOverdue, isOverdue);
    }
    
    public static IEnumerable<object[]> StatusUpdateTestData()
    {
        // Structure: DueDate offset days, Payment amount, Initial status, Expected status
        yield return new object[] { 5, 100.0m, InvoiceStatus.Pending, InvoiceStatus.Paid };      // Fully paid
        yield return new object[] { -5, 0.0m, InvoiceStatus.Pending, InvoiceStatus.Overdue };    // Overdue
        yield return new object[] { 5, 50.0m, InvoiceStatus.Pending, InvoiceStatus.Pending };    // Pending, not overdue
        yield return new object[] { -5, 100.0m, InvoiceStatus.Overdue, InvoiceStatus.Paid };     // Overdue but now paid
    }
    
    [Theory]
    [DisplayName("Update Status - Sets Correct Status Based On Conditions")]
    [MemberData(nameof(StatusUpdateTestData))]
    public void UpdateStatus_WhenConditionsChange_SetsCorrectStatus(
        int dueDateOffsetDays, 
        decimal paymentAmount, 
        InvoiceStatus initialStatus, 
        InvoiceStatus expectedStatus)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = 100.0m,
            Status = initialStatus,
            DueDate = DateTime.Now.AddDays(dueDateOffsetDays),
            Payments = paymentAmount > 0 
                ? new List<Payment> { new Payment { Amount = paymentAmount } }
                : new List<Payment>()
        };
        
        // Act
        invoice.UpdateStatus();
        
        // Assert
        Assert.Equal(expectedStatus, invoice.Status);
    }
    
    public static IEnumerable<object[]> MultiplePaymentsTestData()
    {
        yield return new object[] { 100.0, new double[] { 30.0, 20.0, 50.0 }, 0.0 };  // Multiple payments summing to full amount
        yield return new object[] { 100.0, new double[] { 30.0, 20.0 }, 50.0 };       // Multiple payments summing to partial amount
    }
    
    [Theory]
    [DisplayName("Remaining Amount - Calculates Correctly With Multiple Payments")]
    [MemberData(nameof(MultiplePaymentsTestData))]
    public void GetRemainingAmount_WhenMultiplePaymentsExist_ReturnsCorrectAmount(
        decimal invoiceAmount, 
        double[] paymentAmounts, 
        decimal expectedRemaining)
    {
        // Arrange
        var invoice = new Invoice
        {
            Amount = invoiceAmount,
            Payments = new List<Payment>()
        };
        
        foreach (var amount in paymentAmounts)
        {
            invoice.Payments.Add(new Payment { Amount = (decimal)amount });
        }
        
        // Act
        var remainingAmount = invoice.GetRemainingAmount();
        
        // Assert
        Assert.Equal(expectedRemaining, remainingAmount);
    }
} 