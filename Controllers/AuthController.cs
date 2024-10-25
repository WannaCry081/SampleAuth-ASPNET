using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Controllers.Utils;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Services.Auth;

namespace sample_auth_aspnet.Controllers;

/// <summary>
/// Controller for handling user authentication.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(
    ILogger<AuthController> logger,
    IAuthService authService) : ControllerBase
{

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="authRegister">The registration details for the user.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the registration process.</returns>
    /// <response code="200">Returns if the registration was successful.</response>
    /// <response code="400">Returns if the model is invalid or registration failed.</response>
    /// <response code="500">Returns if an internal server error occurred.</response>
    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser([FromBody] AuthRegisterDto authRegister)
    {
        try
        {
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

    [HttpPost("login")]
    public async Task<IActionResult> LoginUser([FromBody] AuthLoginDto authLogin)
    {
        try
        {
            var response = await authService.LoginUserAsync(authLogin);

            if (!response.Success) 
                return ControllerUtil.GetActionResultFromError(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in logging in to a user.");
            return Problem("An error occurred while processing your request.");
        }
    }
}