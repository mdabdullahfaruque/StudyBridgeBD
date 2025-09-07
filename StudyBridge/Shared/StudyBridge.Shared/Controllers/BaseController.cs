using Microsoft.AspNetCore.Mvc;
using StudyBridge.Shared.Common;
using StudyBridge.Shared.Exceptions;

namespace StudyBridge.Shared.Controllers;

public abstract class BaseController : ControllerBase
{
    protected IActionResult HandleServiceResult<T>(ServiceResult<T> result)
    {
        if (result.IsSuccess)
        {
            return result.StatusCode switch
            {
                200 => Ok(ApiResponse<T>.SuccessResult(result.Data!, result.Message)),
                201 => Created(string.Empty, ApiResponse<T>.SuccessResult(result.Data!, result.Message)),
                204 => NoContent(),
                _ => StatusCode(result.StatusCode, ApiResponse<T>.SuccessResult(result.Data!, result.Message))
            };
        }

        return result.StatusCode switch
        {
            400 => BadRequest(ApiResponse<T>.FailureResult(result.Message, result.Errors)),
            401 => Unauthorized(ApiResponse<T>.FailureResult(result.Message, result.Errors)),
            403 => Forbid(ApiResponse<T>.FailureResult(result.Message, result.Errors).Message),
            404 => NotFound(ApiResponse<T>.FailureResult(result.Message, result.Errors)),
            409 => Conflict(ApiResponse<T>.FailureResult(result.Message, result.Errors)),
            422 => UnprocessableEntity(ApiResponse<T>.FailureResult(result.Message, result.Errors)),
            _ => StatusCode(result.StatusCode, ApiResponse<T>.FailureResult(result.Message, result.Errors))
        };
    }

    protected IActionResult HandleException(StudyBridgeException ex)
    {
        return ex.StatusCode switch
        {
            400 => BadRequest(ApiResponse<object>.FailureResult(ex.Message, ex.Errors)),
            401 => Unauthorized(ApiResponse<object>.FailureResult(ex.Message, ex.Errors)),
            403 => Forbid(ex.Message),
            404 => NotFound(ApiResponse<object>.FailureResult(ex.Message, ex.Errors)),
            409 => Conflict(ApiResponse<object>.FailureResult(ex.Message, ex.Errors)),
            422 => UnprocessableEntity(ApiResponse<object>.FailureResult(ex.Message, ex.Errors)),
            _ => StatusCode(ex.StatusCode, ApiResponse<object>.FailureResult(ex.Message, ex.Errors))
        };
    }
}
