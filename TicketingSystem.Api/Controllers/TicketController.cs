using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Api.Contracts.Tickets;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Domain.Enums;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TicketsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TicketResponse>>> GetTickets(
        [FromQuery] TicketStatus? status,
        [FromQuery] TicketPriority? priority,
        [FromQuery] Guid? assignedToId,
        [FromQuery] Guid? createdById,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Ticket> query = _dbContext.Tickets.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(x => x.Priority == priority.Value);
        }

        if (assignedToId.HasValue)
        {
            query = query.Where(x => x.AssignedToId == assignedToId.Value);
        }

        if (createdById.HasValue)
        {
            query = query.Where(x => x.CreatedById == createdById.Value);
        }

        var tickets = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return Ok(tickets.Select(MapTicket).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketDetailsResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.Tickets
            .AsNoTracking()
            .Include(x => x.Comments)
            .Include(x => x.Attachments)
            .Include(x => x.History)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (ticket is null)
        {
            return NotFound();
        }

        return Ok(MapTicketDetails(ticket));
    }

    [HttpPost]
    public async Task<ActionResult<TicketResponse>> Create(CreateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var creatorExists = await _dbContext.Users.AnyAsync(x => x.Id == request.CreatedById, cancellationToken);
        if (!creatorExists)
        {
            return BadRequest($"User '{request.CreatedById}' was not found.");
        }

        var ticket = new Ticket(request.Title.Trim(), request.Description.Trim(), request.Priority, request.CreatedById);

        await _dbContext.Tickets.AddAsync(ticket, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = ticket.Id }, MapTicket(ticket));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketResponse>> Update(Guid id, UpdateTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        ticket.UpdateDetails(request.Title.Trim(), request.Description.Trim(), request.Priority);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapTicket(ticket));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        _dbContext.Tickets.Remove(ticket);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("{id:guid}/assign")]
    public async Task<ActionResult<TicketResponse>> Assign(Guid id, AssignTicketRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        var assignedUserExists = await _dbContext.Users.AnyAsync(x => x.Id == request.AssignedToId, cancellationToken);
        if (!assignedUserExists)
        {
            return BadRequest($"User '{request.AssignedToId}' was not found.");
        }

        ticket.Assign(request.AssignedToId);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapTicket(ticket));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<TicketResponse>> ChangeStatus(Guid id, ChangeTicketStatusRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await _dbContext.Tickets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (ticket is null)
        {
            return NotFound();
        }

        ticket.ChangeStatus(request.Status);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapTicket(ticket));
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<ActionResult<TicketCommentResponse>> AddComment(
        Guid id,
        AddTicketCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var ticketExists = await _dbContext.Tickets.AnyAsync(x => x.Id == id, cancellationToken);
        if (!ticketExists)
        {
            return NotFound();
        }

        var userExists = await _dbContext.Users.AnyAsync(x => x.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return BadRequest($"User '{request.UserId}' was not found.");
        }

        var comment = new TicketComment(request.Content.Trim(), id, request.UserId);

        await _dbContext.TicketComments.AddAsync(comment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new TicketCommentResponse(
            comment.Id,
            comment.Content,
            comment.TicketId,
            comment.UserId,
            comment.CreatedAt));
    }

    [HttpPost("{id:guid}/attachments")]
    public async Task<ActionResult<TicketAttachmentResponse>> AddAttachment(
        Guid id,
        AddTicketAttachmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var ticketExists = await _dbContext.Tickets.AnyAsync(x => x.Id == id, cancellationToken);
        if (!ticketExists)
        {
            return NotFound();
        }

        var attachment = new TicketAttachment(request.FileName.Trim(), request.FilePath.Trim(), id);

        await _dbContext.TicketAttachments.AddAsync(attachment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new TicketAttachmentResponse(
            attachment.Id,
            attachment.FileName,
            attachment.FilePath,
            attachment.TicketId,
            attachment.CreatedAt));
    }

    private static TicketResponse MapTicket(Ticket ticket)
    {
        return new TicketResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedById,
            ticket.AssignedToId,
            ticket.CreatedAt,
            ticket.UpdatedAt);
    }

    private static TicketDetailsResponse MapTicketDetails(Ticket ticket)
    {
        return new TicketDetailsResponse(
            MapTicket(ticket),
            ticket.Comments
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TicketCommentResponse(x.Id, x.Content, x.TicketId, x.UserId, x.CreatedAt))
                .ToList(),
            ticket.Attachments
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TicketAttachmentResponse(x.Id, x.FileName, x.FilePath, x.TicketId, x.CreatedAt))
                .ToList(),
            ticket.History
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new TicketHistoryResponse(
                    x.Id,
                    x.TicketId,
                    x.ChangedById,
                    x.PropertyChanged,
                    x.OldValue,
                    x.NewValue,
                    x.CreatedAt))
                .ToList());
    }
}
