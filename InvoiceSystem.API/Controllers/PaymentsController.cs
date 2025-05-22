using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly IInvoiceService _invoiceService;
    
    public PaymentsController(IPaymentService paymentService, IInvoiceService invoiceService)
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
    }
    
    [HttpGet("invoice/{invoiceId}")]
    [ProducesResponseType(typeof(IEnumerable<Payment>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Payment>>> GetPaymentsForInvoice(int invoiceId)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var payments = await _paymentService.GetPaymentsForInvoiceAsync(invoiceId);
        return Ok(payments);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Payment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Payment>> GetPayment(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        
        if (payment == null)
        {
            return NotFound();
        }
        
        return Ok(payment);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Payment), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Payment>> RecordPayment(Payment payment)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(payment.InvoiceId);
        if (invoice == null)
        {
            return NotFound($"Invoice with ID {payment.InvoiceId} not found.");
        }
        
        var createdPayment = await _paymentService.RecordPaymentAsync(payment);
        return CreatedAtAction(nameof(GetPayment), new { id = createdPayment.Id }, createdPayment);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePayment(int id, Payment payment)
    {
        if (id != payment.Id)
        {
            return BadRequest();
        }
        
        var existingPayment = await _paymentService.GetPaymentByIdAsync(id);
        if (existingPayment == null)
        {
            return NotFound();
        }
        
        await _paymentService.UpdatePaymentAsync(payment);
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayment(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound();
        }
        
        await _paymentService.DeletePaymentAsync(id);
        return NoContent();
    }
    
    [HttpGet("invoice/{invoiceId}/total")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> GetTotalPaymentsForInvoice(int invoiceId)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var total = await _paymentService.GetTotalPaymentsForInvoiceAsync(invoiceId);
        return Ok(total);
    }
} 