namespace StudyBridge.Shared.Common;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public List<string> Errors { get; private set; } = new();
    public int StatusCode { get; private set; }

    private ServiceResult() { }

    public static ServiceResult<T> Success(T data, string message = "", int statusCode = 200)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message,
            StatusCode = statusCode
        };
    }

    public static ServiceResult<T> Failure(string message, int statusCode = 400, List<string>? errors = null)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors ?? new List<string> { message }
        };
    }

    public static ServiceResult<T> Failure(List<string> errors, int statusCode = 400, string message = "Operation failed")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Errors = errors
        };
    }
}
