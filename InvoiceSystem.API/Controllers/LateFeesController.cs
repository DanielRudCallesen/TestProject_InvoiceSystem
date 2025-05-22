using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LateFeesController : ControllerBase
{
    private readonly ILateFeeService _lateFeeService;
    private readonly IInvoiceService _invoiceService;
    
    public LateFeesController(ILateFeeService lateFeeService, IInvoiceService invoiceService)
    {
        _lateFeeService = lateFeeService;
        _invoiceService = invoiceService;
    }
    
    [HttpGet("invoice/{invoiceId}")]
    [ProducesResponseType(typeof(IEnumerable<LateFee>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<LateFee>>> GetLateFeesForInvoice(int invoiceId)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var lateFees = await _lateFeeService.GetLateFeesForInvoiceAsync(invoiceId);
        return Ok(lateFees);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LateFee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LateFee>> GetLateFee(int id)
    {
        var lateFee = await _lateFeeService.GetLateFeeByIdAsync(id);
        
        if (lateFee == null)
        {
            return NotFound();
        }
        
        return Ok(lateFee);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(LateFee), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LateFee>> ApplyLateFee(LateFee lateFee)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(lateFee.InvoiceId);
        if (invoice == null)
        {
            return NotFound($"Invoice with ID {lateFee.InvoiceId} not found.");
        }
        
        var createdLateFee = await _lateFeeService.ApplyLateFeeAsync(lateFee);
        return CreatedAtAction(nameof(GetLateFee), new { id = createdLateFee.Id }, createdLateFee);
    }
    
    [HttpGet("calculate/{invoiceId}")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> CalculateLateFee(int invoiceId, [FromQuery] decimal feePercentage = 1.0m)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var feeAmount = await _lateFeeService.CalculateLateFeeAsync(invoiceId, DateTime.Now, feePercentage);
        return Ok(feeAmount);
    }
    
    [HttpGet("invoices-needing-fees")]
    [ProducesResponseType(typeof(IEnumerable<Invoice>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoicesForLateFeeApplication()
    {
        var invoices = await _lateFeeService.GetInvoicesForLateFeeApplicationAsync();
        return Ok(invoices);
    }
} 