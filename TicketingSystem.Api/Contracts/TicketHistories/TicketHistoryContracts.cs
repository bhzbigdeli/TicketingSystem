using System.ComponentModel.DataAnnotations;

namespace TicketingSystem.Api.Contracts.TicketHistories;

public sealed class CreateTicketHistoryRequest
{
    [Required]
    public Guid TicketId { get; init; }

    [Required]
    public Guid ChangedById { get; init; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string PropertyChanged { get; init; } = string.Empty;

    [StringLength(1000)]
    public string OldValue { get; init; } = string.Empty;

    [StringLength(1000)]
    public string NewValue { get; init; } = string.Empty;
}

public sealed class UpdateTicketHistoryRequest
{
    [Required]
    public Guid ChangedById { get; init; }

    [Required, StringLength(200, MinimumLength = 1)]
    public string PropertyChanged { get; init; } = string.Empty;

    [StringLength(1000)]
    public string OldValue { get; init; } = string.Empty;

    [StringLength(1000)]
    public string NewValue { get; init; } = string.Empty;
}

public sealed record TicketHistoryResponse(
    Guid Id,
    Guid TicketId,
    Guid ChangedById,
    string PropertyChanged,
    string OldValue,
    string NewValue,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
