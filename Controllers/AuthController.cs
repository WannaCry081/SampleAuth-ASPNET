using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Controllers.Utils;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Services.Auth;

namespace sample_auth_aspnet.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(
    ILogger<AuthController> logger,
    IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] AuthRegisterDto authRegister)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await authService.RegisterUserAsync(authRegister);

            if (!response.Success)
                return ControllerUtil.GetActionResultFromError(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in registering user details.");
            return Problem("An error occurred while processing your request.");
        }
    }
}