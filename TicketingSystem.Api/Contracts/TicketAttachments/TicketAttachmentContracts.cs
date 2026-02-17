using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Api.Contracts.TicketAttachments;

public sealed class CreateTicketAttachmentRequest
{
    [Required, StringLength(300, MinimumLength = 1)]
    public string FileName { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string FilePath { get; init; } = string.Empty;

    [Required]
    public Guid TicketId { get; init; }
}

public sealed class UpdateTicketAttachmentRequest
{
    [Required, StringLength(300, MinimumLength = 1)]
    public string FileName { get; init; } = string.Empty;

    [Required, MinLength(1)]
    public string FilePath { get; init; } = string.Empty;
}

public sealed record TicketAttachmentResponse(
    Guid Id,
    string FileName,
    string FilePath,
    Guid TicketId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
