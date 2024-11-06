using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using sample_auth_aspnet.Models.Response;
using static sample_auth_aspnet.Models.Utils.Error;

namespace sample_auth_aspnet.Controllers.Utils;

public static class ControllerUtil
{
    public static ApiResponse<T> ValidateRequest<T>(ModelStateDictionary modelState)
    {
        var details = modelState
            .Where(ms => ms.Value!.Errors.Count > 0)
            .ToDictionary(
                kvp => char.ToLower(kvp.Key[0]) + kvp.Key[1..],
                kvp => string.Join("; ", kvp.Value!.Errors.Select(e => e.ErrorMessage))
            );

        return ApiResponse<T>.ErrorResponse(
            ValidationError,
            ErrorType.ValidationError,
            details
        );
    }

    public static int GetUserId(ClaimsPrincipal user)
    {
        var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(userIdString, out var userId) ? userId : -1;
    }

    public static IActionResult GetActionResultFromError<T>(ApiResponse<T> apiResponse)
    {
        var errorType = apiResponse.Title;

        return errorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(apiResponse),
            ErrorType.BadRequest => new BadRequestObjectResult(apiResponse),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(apiResponse),
            ErrorType.ValidationError => new BadRequestObjectResult(apiResponse),
            ErrorType.InternalServer => new StatusCodeResult(500),
            _ => new BadRequestObjectResult(apiResponse)
        };
    }
}
