using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TaskManagement.Api.Auth;
using TaskManagement.Api.Data;
using TaskManagement.Api.Dtos;
using TaskManagement.Api.Entities;
using TaskManagement.Api.Exceptions;
using TaskManagement.Api.Services;

namespace TaskManagement.Api.Tests.Services;

public class AuthServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"auth-tests-{Guid.NewGuid()}")
            .Options;
        _db = new AppDbContext(options);

        var tokenService = new JwtTokenService(
            Options.Create(new JwtSettings
            {
                Issuer = "test",
                Audience = "test",
                Key = "unit-test-signing-key-0123456789abcdef"
            }),
            TimeProvider.System);

        _service = new AuthService(_db, new PasswordHasher<User>(), tokenService, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task Register_CreatesUserWithHashedPasswordAndUserRole()
    {
        var response = await _service.RegisterAsync(new RegisterRequest("Jane Doe", "Jane@Example.com", "Passw0rd!"));

        var user = await _db.Users.SingleAsync();
        Assert.Equal("jane@example.com", user.Email);
        Assert.Equal(UserRole.User, user.Role);
        Assert.NotEqual("Passw0rd!", user.PasswordHash);
        Assert.Equal("User", response.Role);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsConflict()
    {
        await _service.RegisterAsync(new RegisterRequest("Jane Doe", "jane@example.com", "Passw0rd!"));

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.RegisterAsync(new RegisterRequest("Another Jane", "JANE@example.com", "Passw0rd!")));
    }

    [Fact]
    public async Task Login_WithCorrectPassword_ReturnsToken()
    {
        await _service.RegisterAsync(new RegisterRequest("Jane Doe", "jane@example.com", "Passw0rd!"));

        var response = await _service.LoginAsync(new LoginRequest("jane@example.com", "Passw0rd!"));

        Assert.Equal("jane@example.com", response.Email);
        Assert.False(string.IsNullOrWhiteSpace(response.Token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        await _service.RegisterAsync(new RegisterRequest("Jane Doe", "jane@example.com", "Passw0rd!"));

        await Assert.ThrowsAsync<UnauthorizedAppException>(() =>
            _service.LoginAsync(new LoginRequest("jane@example.com", "wrong-password")));
    }

    public void Dispose()
    {
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}
