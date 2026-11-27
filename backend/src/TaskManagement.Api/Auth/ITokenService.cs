using TaskManagement.Api.Entities;

namespace TaskManagement.Api.Auth;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
