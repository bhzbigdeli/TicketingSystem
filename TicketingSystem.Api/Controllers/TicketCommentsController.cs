using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Api.Contracts.TicketComments;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketCommentsController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TicketCommentsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TicketCommentResponse>>> GetAll(
        [FromQuery] Guid? ticketId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TicketComment> query = _dbContext.TicketComments.AsNoTracking();

        if (ticketId.HasValue)
        {
            query = query.Where(x => x.TicketId == ticketId.Value);
        }

        var comments = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(comments.Select(MapComment).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketCommentResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.TicketComments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (comment is null)
        {
            return NotFound();
        }

        return Ok(MapComment(comment));
    }

    [HttpPost]
    public async Task<ActionResult<TicketCommentResponse>> Create(CreateTicketCommentRequest request, CancellationToken cancellationToken = default)
    {
        var ticketExists = await _dbContext.Tickets.AnyAsync(x => x.Id == request.TicketId, cancellationToken);
        if (!ticketExists)
        {
            return BadRequest($"Ticket '{request.TicketId}' was not found.");
        }

        var userExists = await _dbContext.Users.AnyAsync(x => x.Id == request.UserId, cancellationToken);
        if (!userExists)
        {
            return BadRequest($"User '{request.UserId}' was not found.");
        }

        var comment = new TicketComment(request.Content.Trim(), request.TicketId, request.UserId);
        await _dbContext.TicketComments.AddAsync(comment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = comment.Id }, MapComment(comment));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketCommentResponse>> Update(
        Guid id,
        UpdateTicketCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.TicketComments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (comment is null)
        {
            return NotFound();
        }

        comment.UpdateContent(request.Content.Trim());
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapComment(comment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.TicketComments.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (comment is null)
        {
            return NotFound();
        }

        _dbContext.TicketComments.Remove(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static TicketCommentResponse MapComment(TicketComment comment)
    {
        return new TicketCommentResponse(
            comment.Id,
            comment.Content,
            comment.TicketId,
            comment.UserId,
            comment.CreatedAt,
            comment.UpdatedAt);
    }
}
