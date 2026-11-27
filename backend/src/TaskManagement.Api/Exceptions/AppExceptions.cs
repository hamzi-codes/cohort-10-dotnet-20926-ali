using System.Net;

namespace TaskManagement.Api.Exceptions;

/// <summary>
/// Base type for expected application errors. The global exception middleware
/// turns these into ProblemDetails responses with the given status code.
/// </summary>
public abstract class AppException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}

public sealed class NotFoundException(string message) : AppException(message, HttpStatusCode.NotFound);

public sealed class ConflictException(string message) : AppException(message, HttpStatusCode.Conflict);

public sealed class BadRequestException(string message) : AppException(message, HttpStatusCode.BadRequest);

public sealed class UnauthorizedAppException(string message) : AppException(message, HttpStatusCode.Unauthorized);

public sealed class ForbiddenException(string message) : AppException(message, HttpStatusCode.Forbidden);
