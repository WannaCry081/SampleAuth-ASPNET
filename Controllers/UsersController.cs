using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Controllers.Utils;
using sample_auth_aspnet.Services.Users;

namespace sample_auth_aspnet.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController(
    ILogger<UsersController> logger,
    IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
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
