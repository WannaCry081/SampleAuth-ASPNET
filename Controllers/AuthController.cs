using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Controllers.Utils;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Dtos.Reponse;
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
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<AuthDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
        Type = typeof(ErrorResponseDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser([FromBody] AuthRegisterDto authRegister)
    {
        try
        {
            var response = await authService.RegisterUserAsync(authRegister);

            if (response.Status.Equals("error"))
                return ControllerUtil.GetActionResultFromError(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in registering user details.");
            return Problem("An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Authenticates registered user.
    /// </summary>
    /// <param name="authLogin">The login details for the user, including email and password.</param>
    /// <returns>A response containing the access token if authentication is successful.</returns>
    /// <response code="200">Indicates successful login and returns an access token.</response>
    /// <response code="400">Indicates that the login details are incorrect or missing required fields.</response>
    /// <response code="500">Indicates an internal server error during processing.</response>
    [HttpPost("login")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<AuthDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(ErrorResponseDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginUser([FromBody] AuthLoginDto authLogin)
    {
        try
        {
            var response = await authService.LoginUserAsync(authLogin);

            if (response.Status.Equals("error"))
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