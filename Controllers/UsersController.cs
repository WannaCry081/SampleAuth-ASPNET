using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Controllers.Utils;
using sample_auth_aspnet.Models.Dtos.Reponse;
using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Services.Users;

namespace sample_auth_aspnet.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController(
    ILogger<UsersController> logger,
    IUserService userService) : ControllerBase
{
    /// <summary>
    ///     Fetches the authenticated user's details.
    /// </summary>
    /// <returns>
    ///     Returns an <see cref="IActionResult"/> containing:
    ///     - <see cref="OkObjectResult"/> with the user's details.
    ///     - <see cref="UnauthorizedObjectResult"/> if the request is invalid.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="200">Returns the user's details.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<UserDetailsDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserDetails()
    {
        var userId = ControllerUtil.GetUserId(User);
        if (userId == -1)
        {
            logger.LogWarning("Unauthorized access attempt.");
            return Unauthorized(new { message = "User is not authenticated." });
        }

        logger.LogInformation("Fetching user details for userId: {UserId}", userId);

        try
        {
            var response = await userService.GetUserDetailsAsync(userId);

            logger.LogInformation("Successfully fetched user details for userId: {UserId}", userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching user details for userId: {UserId}", userId);
            return Problem("An internal server error occurred. Please try again later.");
        }
    }
}
