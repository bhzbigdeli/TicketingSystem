using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Api.Contracts.Users;
using TicketingSystem.Domain.Entities;
using TicketingSystem.Infrastructure.Data;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _dbContext;

    public UsersController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<UserResponse>>> GetAll(CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .ToListAsync(cancellationToken);

        return Ok(users.Select(MapUser).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(MapUser(user));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim();
        var emailExists = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            return Conflict($"Email '{normalizedEmail}' already exists.");
        }

        var user = new User(request.FullName.Trim(), normalizedEmail, request.PasswordHash.Trim(), request.Role);
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapUser(user));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var normalizedEmail = request.Email.Trim();
        var emailInUse = await _dbContext.Users.AnyAsync(x => x.Email == normalizedEmail && x.Id != id, cancellationToken);
        if (emailInUse)
        {
            return Conflict($"Email '{normalizedEmail}' already exists.");
        }

        user.UpdateProfile(request.FullName.Trim(), normalizedEmail);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapUser(user));
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<UserResponse>> ChangeRole(Guid id, ChangeUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.ChangeRole(request.Role);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapUser(user));
    }

    [HttpPatch("{id:guid}/password")]
    public async Task<ActionResult<UserResponse>> ChangePassword(Guid id, ChangeUserPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.ChangePassword(request.NewPasswordHash.Trim());
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(MapUser(user));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static UserResponse MapUser(User user)
    {
        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.Role,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
