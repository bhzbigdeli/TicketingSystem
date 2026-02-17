using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Api.Contracts.TicketAttachments;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketAttachmentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TicketAttachmentsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TicketAttachmentResponse>>> GetAll(
        [FromQuery] Guid? ticketId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TicketAttachment> query = _dbContext.TicketAttachments.AsNoTracking();

        if (ticketId.HasValue)
        {
            query = query.Where(x => x.TicketId == ticketId.Value);
        }

        var attachments = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(attachments.Select(MapAttachment).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketAttachmentResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var attachment = await _dbContext.TicketAttachments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null)
        {
            return NotFound();
        }

        return Ok(MapAttachment(attachment));
    }

    [HttpPost]
    public async Task<ActionResult<TicketAttachmentResponse>> Create(CreateTicketAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        var ticketExists = await _dbContext.Tickets.AnyAsync(x => x.Id == request.TicketId, cancellationToken);
        if (!ticketExists)
        {
            return BadRequest($"Ticket '{request.TicketId}' was not found.");
        }

        var attachment = new TicketAttachment(request.FileName.Trim(), request.FilePath.Trim(), request.TicketId);
        await _dbContext.TicketAttachments.AddAsync(attachment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = attachment.Id }, MapAttachment(attachment));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketAttachmentResponse>> Update(
        Guid id,
        UpdateTicketAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var attachment = await _dbContext.TicketAttachments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null)
        {
            return NotFound();
        }

        attachment.UpdateFile(request.FileName.Trim(), request.FilePath.Trim());
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapAttachment(attachment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var attachment = await _dbContext.TicketAttachments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (attachment is null)
        {
            return NotFound();
        }

        _dbContext.TicketAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static TicketAttachmentResponse MapAttachment(TicketAttachment attachment)
    {
        return new TicketAttachmentResponse(
            attachment.Id,
            attachment.FileName,
            attachment.FilePath,
            attachment.TicketId,
            attachment.CreatedAt,
            attachment.UpdatedAt);
    }
}
