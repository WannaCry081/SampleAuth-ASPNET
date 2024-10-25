using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Models.Response;
using static sample_auth_aspnet.Models.Utils.Errors;

namespace sample_auth_aspnet.Controllers.Utils;

public static class ControllerUtil
{
    public static IActionResult GetActionResultFromError<T>(ApiResponse<T> apiResponse)
    {
        var errorType = apiResponse.ErrorType;

        return errorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(apiResponse),
            ErrorType.BadRequest => new BadRequestObjectResult(apiResponse),
            ErrorType.Unauthorized => new UnauthorizedResult(),
            ErrorType.ValidationError => new BadRequestObjectResult(apiResponse),
            ErrorType.InternalServer => new StatusCodeResult(500),
            _ => new BadRequestObjectResult(apiResponse)
        };
    }
}
