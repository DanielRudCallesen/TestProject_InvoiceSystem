using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    
    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Invoice>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices()
    {
        var invoices = await _invoiceService.GetAllInvoicesAsync();
        return Ok(invoices);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Invoice), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Invoice>> GetInvoice(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        
        if (invoice == null)
        {
            return NotFound();
        }
        
        return Ok(invoice);
    }
    
    [HttpGet("{id}/details")]
    [ProducesResponseType(typeof(Invoice), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Invoice>> GetInvoiceWithDetails(int id)
    {
        var invoice = await _invoiceService.GetInvoiceWithDetailsAsync(id);
        
        if (invoice == null)
        {
            return NotFound();
        }
        
        return Ok(invoice);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Invoice), StatusCodes.Status201Created)]
    public async Task<ActionResult<Invoice>> CreateInvoice(Invoice invoice)
    {
        var createdInvoice = await _invoiceService.CreateInvoiceAsync(invoice);
        return CreatedAtAction(nameof(GetInvoice), new { id = createdInvoice.Id }, createdInvoice);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInvoice(int id, Invoice invoice)
    {
        if (id != invoice.Id)
        {
            return BadRequest();
        }
        
        var existingInvoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (existingInvoice == null)
        {
            return NotFound();
        }
        
        await _invoiceService.UpdateInvoiceAsync(invoice);
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInvoice(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }
        
        await _invoiceService.DeleteInvoiceAsync(id);
        return NoContent();
    }
    
    [HttpGet("{id}/remaining-amount")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> GetRemainingAmount(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var remainingAmount = await _invoiceService.CalculateRemainingAmountAsync(id);
        return Ok(remainingAmount);
    }
    
    [HttpGet("{id}/is-paid")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> IsInvoicePaid(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var isPaid = await _invoiceService.IsInvoicePaidAsync(id);
        return Ok(isPaid);
    }
    
    [HttpGet("{id}/is-overdue")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<bool>> IsInvoiceOverdue(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var isOverdue = await _invoiceService.IsInvoiceOverdueAsync(id);
        return Ok(isOverdue);
    }
}