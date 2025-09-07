namespace StudyBridge.Shared.Exceptions;

public abstract class StudyBridgeException : Exception
{
    public int StatusCode { get; }
    public List<string> Errors { get; }

    protected StudyBridgeException(string message, int statusCode = 400, List<string>? errors = null) 
        : base(message)
    {
        StatusCode = statusCode;
        Errors = errors ?? new List<string> { message };
    }

    protected StudyBridgeException(string message, Exception innerException, int statusCode = 400, List<string>? errors = null) 
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Errors = errors ?? new List<string> { message };
    }
}

public class ValidationException : StudyBridgeException
{
    public ValidationException(string message, List<string>? errors = null) 
        : base(message, 400, errors)
    {
    }

    public ValidationException(List<string> errors) 
        : base("Validation failed", 400, errors)
    {
    }
}

public class NotFoundException : StudyBridgeException
{
    public NotFoundException(string message) 
        : base(message, 404)
    {
    }

    public NotFoundException(string entityName, object id) 
        : base($"{entityName} with id '{id}' was not found", 404)
    {
    }
}

public class UnauthorizedException : StudyBridgeException
{
    public UnauthorizedException(string message = "Unauthorized access") 
        : base(message, 401)
    {
    }
}

public class ForbiddenException : StudyBridgeException
{
    public ForbiddenException(string message = "Access forbidden") 
        : base(message, 403)
    {
    }
}

public class ConflictException : StudyBridgeException
{
    public ConflictException(string message) 
        : base(message, 409)
    {
    }
}

public class BusinessLogicException : StudyBridgeException
{
    public BusinessLogicException(string message) 
        : base(message, 422)
    {
    }
}
