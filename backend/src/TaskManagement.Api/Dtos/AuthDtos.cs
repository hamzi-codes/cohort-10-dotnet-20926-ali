using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Dtos;

public record RegisterRequest(
    [Required, StringLength(100, MinimumLength = 2)] string FullName,
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 8)] string Password);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record AuthResponse(int UserId, string FullName, string Email, string Role, string Token, DateTime ExpiresAt);
