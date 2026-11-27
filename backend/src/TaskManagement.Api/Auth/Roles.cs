using TaskManagement.Api.Entities;

namespace TaskManagement.Api.Auth;

/// <summary>Role names used in JWT claims and [Authorize(Roles = ...)] attributes.</summary>
public static class Roles
{
    public const string Admin = nameof(UserRole.Admin);
    public const string User = nameof(UserRole.User);
}
