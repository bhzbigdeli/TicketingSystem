using System.ComponentModel.DataAnnotations;
using TicketingSystem.Domain.Enums;

namespace TicketingSystem.Api.Contracts.Tickets;

public sealed class CreateTicketRequest
{
    [Required, StringLength(300, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required, MinLength(3)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public TicketPriority Priority { get; init; }

    [Required]
    public Guid CreatedById { get; init; }
}

public sealed class UpdateTicketRequest
{
    [Required, StringLength(300, MinimumLength = 3)]
    public string Title { get; init; } = string.Empty;

    [Required, MinLength(3)]
    public string Description { get; init; } = string.Empty;

    [Required]
    public TicketPriority Priority { get; init; }
}

public sealed class AssignTicketRequest
{
    [Required]
    public Guid AssignedToId { get; init; }
}

public sealed class ChangeTicketStatusRequest
{
    [Required]
    public TicketStatus Status { get; init; }
}

public sealed class AddTicketCommentRequest
{
    [Required, StringLength(2000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;

    [Required]
    public Guid UserId { get; init; }
}

public sealed class AddTicketAttachmentRequest
{
    [Required, StringLength(300, MinimumLength = 1)]
    public string FileName { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string FilePath { get; init; } = string.Empty;
}

public sealed record TicketResponse(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid CreatedById,
    Guid? AssignedToId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record TicketCommentResponse(
    Guid Id,
    string Content,
    Guid TicketId,
    Guid UserId,
    DateTime CreatedAt);

public sealed record TicketAttachmentResponse(
    Guid Id,
    string FileName,
    string FilePath,
    Guid TicketId,
    DateTime CreatedAt);

public sealed record TicketHistoryResponse(
    Guid Id,
    Guid TicketId,
    Guid ChangedById,
    string PropertyChanged,
    string OldValue,
    string NewValue,
    DateTime CreatedAt);

public sealed record TicketDetailsResponse(
    TicketResponse Ticket,
    IReadOnlyCollection<TicketCommentResponse> Comments,
    IReadOnlyCollection<TicketAttachmentResponse> Attachments,
    IReadOnlyCollection<TicketHistoryResponse> History);
