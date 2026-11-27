using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using TaskManagement.Api.Auth;
using TaskManagement.Api.Entities;

namespace TaskManagement.Api.Tests.Auth;

public class JwtTokenServiceTests
{
    private static readonly JwtSettings Settings = new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        Key = "unit-test-signing-key-0123456789abcdef",
        ExpiryMinutes = 30
    };

    [Theory]
    [InlineData(UserRole.User, "User")]
    [InlineData(UserRole.Admin, "Admin")]
    public void CreateToken_IncludesRoleIssuerAndAudience(UserRole role, string expectedRole)
    {
        var service = new JwtTokenService(Options.Create(Settings), TimeProvider.System);
        var user = new User { Id = 42, FullName = "Jane Doe", Email = "jane@example.com", Role = role };

        var (token, expiresAt) = service.CreateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        Assert.Equal("test-issuer", jwt.Issuer);
        Assert.Contains("test-audience", jwt.Audiences);
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == expectedRole);
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "42");
        Assert.InRange(expiresAt, DateTime.UtcNow.AddMinutes(29), DateTime.UtcNow.AddMinutes(31));
    }
}
