using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Controllers.Utils;
using sample_auth_aspnet.Models.Dtos.Reponse;
using sample_auth_aspnet.Models.Dtos.Users;
using sample_auth_aspnet.Services.Users;

namespace sample_auth_aspnet.Controllers;

/// <summary>
/// Controller for handling authenticated users.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController(
    ILogger<UsersController> logger,
    IUserService userService) : ControllerBase
{
    /// <summary>
    /// Fetches the authenticated user's details.
    /// </summary>
    /// <returns>An <see cref="IActionResult"/> indicating the result of retrieving user's details.</returns>
    /// <response code="200">Returns if fetching user's details was successful.</response>
    /// <response code="401">Returns if the model is not authenticated.</response>
    /// <response code="500">Returns if an internal server error occurred.</response>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<UserDetailsDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserDetails()
    {
        try
        {
            var userId = ControllerUtil.GetUserId(User);

            if (userId == -1)
                return Unauthorized();

            var response = await userService.GetUserDetailsAsync(userId);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in fetching user's details.");
            return Problem("An error occurred while processing your request.");
        }
    }
}
