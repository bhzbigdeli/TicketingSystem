using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Api.Contracts.TicketHistories;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketHistoriesController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public TicketHistoriesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TicketHistoryResponse>>> GetAll(
        [FromQuery] Guid? ticketId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TicketHistory> query = _dbContext.TicketHistories.AsNoTracking();

        if (ticketId.HasValue)
        {
            query = query.Where(x => x.TicketId == ticketId.Value);
        }

        var history = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return Ok(history.Select(MapHistory).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TicketHistoryResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var history = await _dbContext.TicketHistories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (history is null)
        {
            return NotFound();
        }

        return Ok(MapHistory(history));
    }

    [HttpPost]
    public async Task<ActionResult<TicketHistoryResponse>> Create(CreateTicketHistoryRequest request, CancellationToken cancellationToken = default)
    {
        var ticketExists = await _dbContext.Tickets.AnyAsync(x => x.Id == request.TicketId, cancellationToken);
        if (!ticketExists)
        {
            return BadRequest($"Ticket '{request.TicketId}' was not found.");
        }

        var changedByUserExists = await _dbContext.Users.AnyAsync(x => x.Id == request.ChangedById, cancellationToken);
        if (!changedByUserExists)
        {
            return BadRequest($"User '{request.ChangedById}' was not found.");
        }

        var history = new TicketHistory(
            request.TicketId,
            request.ChangedById,
            request.PropertyChanged.Trim(),
            request.OldValue?.Trim() ?? string.Empty,
            request.NewValue?.Trim() ?? string.Empty);

        await _dbContext.TicketHistories.AddAsync(history, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = history.Id }, MapHistory(history));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TicketHistoryResponse>> Update(
        Guid id,
        UpdateTicketHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var history = await _dbContext.TicketHistories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (history is null)
        {
            return NotFound();
        }

        var changedByUserExists = await _dbContext.Users.AnyAsync(x => x.Id == request.ChangedById, cancellationToken);
        if (!changedByUserExists)
        {
            return BadRequest($"User '{request.ChangedById}' was not found.");
        }

        history.UpdateChange(
            request.ChangedById,
            request.PropertyChanged.Trim(),
            request.OldValue?.Trim() ?? string.Empty,
            request.NewValue?.Trim() ?? string.Empty);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapHistory(history));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var history = await _dbContext.TicketHistories.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (history is null)
        {
            return NotFound();
        }

        _dbContext.TicketHistories.Remove(history);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static TicketHistoryResponse MapHistory(TicketHistory history)
    {
        return new TicketHistoryResponse(
            history.Id,
            history.TicketId,
            history.ChangedById,
            history.PropertyChanged,
            history.OldValue,
            history.NewValue,
            history.CreatedAt,
            history.UpdatedAt);
    }
}
