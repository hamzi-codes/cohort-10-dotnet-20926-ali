using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManagement.Api.Exceptions;
using TaskManagement.Api.Middleware;

namespace TaskManagement.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    private sealed class FakeHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;
        public string ApplicationName { get; set; } = "Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }

    private static async Task<(int Status, JsonElement Body)> InvokeAsync(Exception exception, string environment = "Production")
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            new FakeHostEnvironment(environment));

        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);
        return (context.Response.StatusCode, doc.RootElement.Clone());
    }

    [Fact]
    public async Task NotFoundException_Returns404ProblemDetails()
    {
        var (status, body) = await InvokeAsync(new NotFoundException("Task 7 was not found."));

        Assert.Equal(StatusCodes.Status404NotFound, status);
        Assert.Equal(404, body.GetProperty("status").GetInt32());
        Assert.Equal("Task 7 was not found.", body.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task UnexpectedException_Returns500WithoutLeakingDetailsInProduction()
    {
        var (status, body) = await InvokeAsync(new InvalidOperationException("secret internals"));

        Assert.Equal(StatusCodes.Status500InternalServerError, status);
        Assert.False(body.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String
            && detail.GetString()!.Contains("secret internals"));
    }
}
