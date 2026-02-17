using System.ComponentModel.DataAnnotations;
using TicketingSystem.Domain.Enums;

namespace TicketingSystem.Api.Contracts.Users;

public sealed class CreateUserRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(6)]
    public string PasswordHash { get; init; } = string.Empty;

    public UserRole Role { get; init; }
}

public sealed class UpdateUserRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; init; } = string.Empty;
}

public sealed class ChangeUserRoleRequest
{
    public UserRole Role { get; init; }
}

public sealed class ChangeUserPasswordRequest
{
    [Required, MinLength(6)]
    public string NewPasswordHash { get; init; } = string.Empty;
}

public sealed record UserResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
