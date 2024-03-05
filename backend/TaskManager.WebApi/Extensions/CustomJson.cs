using ErrorOr;
using TaskManager.WebApi.Exceptions;

namespace TaskManager.WebApi.Extensions;

public static class CustomResults
{
    public static IResult ErrorJson(int code, List<Error> errors)
    {
        return Results.Json(statusCode: code, data: new
        {
            status_code = code,
            errors = errors.Select(e => e.Description)
        });
    }
    
    public static ApiException GetApiException(Error error)
    {
        return error.Type switch
        {
            ErrorType.Unauthorized => new ApiException(401, error.Description),
            ErrorType.NotFound => new ApiException(404, error.Description),
            _ => new ApiException(500, error.Description)
        };
    }

    public static IResult Ok<T>(ErrorOr<T> result)
    {
        if (result.IsError)
        {
            throw GetApiException(result.FirstError);
        }

        return Results.Ok();
    }
}