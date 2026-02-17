using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Api.Contracts.TicketComments;

public sealed class CreateTicketCommentRequest
{
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;

    [Required]
    public Guid TicketId { get; init; }

    [Required]
    public Guid UserId { get; init; }
}

public sealed class UpdateTicketCommentRequest
{
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;
}

public sealed record TicketCommentResponse(
    Guid Id,
    string Content,
    Guid TicketId,
    Guid UserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
