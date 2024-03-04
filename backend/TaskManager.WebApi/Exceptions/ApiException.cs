namespace TaskManager.WebApi.Exceptions;

public record ApiError(string Details, string? Property = null)
{
    public string Message => Property is null ? Details : string.Format(Details, Property);
}

public class ApiException : Exception
{
    public IEnumerable<ApiError> Errors { get; set; }
    public int Status { get; set; }

    public ApiException(int status, IEnumerable<ApiError> errors)
    {
        Status = status;
        Errors = errors;
    }

    public ApiException(int status, string error)
    {
        Status = status;
        Errors = [new(error)];
    }
}