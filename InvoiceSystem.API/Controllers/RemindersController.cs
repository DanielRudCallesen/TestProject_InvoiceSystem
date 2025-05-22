using InvoiceSystem.Domain.Entities;
using InvoiceSystem.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly IReminderService _reminderService;
    private readonly IInvoiceService _invoiceService;
    
    public RemindersController(IReminderService reminderService, IInvoiceService invoiceService)
    {
        _reminderService = reminderService;
        _invoiceService = invoiceService;
    }
    
    [HttpGet("invoice/{invoiceId}")]
    [ProducesResponseType(typeof(IEnumerable<Reminder>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Reminder>>> GetRemindersForInvoice(int invoiceId)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
        {
            return NotFound();
        }
        
        var reminders = await _reminderService.GetRemindersForInvoiceAsync(invoiceId);
        return Ok(reminders);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Reminder), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Reminder>> GetReminder(int id)
    {
        var reminder = await _reminderService.GetReminderByIdAsync(id);
        
        if (reminder == null)
        {
            return NotFound();
        }
        
        return Ok(reminder);
    }
    
    [HttpPost]
    [ProducesResponseType(typeof(Reminder), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Reminder>> CreateReminder(Reminder reminder)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(reminder.InvoiceId);
        if (invoice == null)
        {
            return NotFound($"Invoice with ID {reminder.InvoiceId} not found.");
        }
        
        var createdReminder = await _reminderService.CreateReminderAsync(reminder);
        return CreatedAtAction(nameof(GetReminder), new { id = createdReminder.Id }, createdReminder);
    }
    
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateReminder(int id, Reminder reminder)
    {
        if (id != reminder.Id)
        {
            return BadRequest();
        }
        
        var existingReminder = await _reminderService.GetReminderByIdAsync(id);
        if (existingReminder == null)
        {
            return NotFound();
        }
        
        await _reminderService.UpdateReminderAsync(reminder);
        return NoContent();
    }
    
    [HttpPost("send")]
    [ProducesResponseType(typeof(Reminder), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Reminder>> SendReminder(int invoiceId, ReminderType reminderType, string message)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
        {
            return NotFound($"Invoice with ID {invoiceId} not found.");
        }
        
        await _reminderService.SendReminderAsync(invoiceId, reminderType, message);
        return Ok();
    }
    
    [HttpGet("invoices-needing-reminders")]
    [ProducesResponseType(typeof(IEnumerable<Invoice>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoicesNeedingReminders([FromQuery] int daysBefore = 3, [FromQuery] int daysAfter = 7)
    {
        var invoices = await _reminderService.GetInvoicesNeedingRemindersAsync(daysBefore, daysAfter);
        return Ok(invoices);
    }
} 