using Microsoft.AspNetCore.Authorization;
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
            logger.LogInformation("User registration attempt for email: {Email}", authRegister.Email);
            var response = await authService.RegisterUserAsync(authRegister);

            if (response.Status.Equals("error"))
            {
                logger.LogWarning("User registration failed for email: {Email}.", authRegister.Email);
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("User registration successful for email: {Email}", authRegister.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in registering user details for email: {Email}", authRegister.Email);
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
            logger.LogInformation("User login attempt for email: {Email}", authLogin.Email);
            var response = await authService.LoginUserAsync(authLogin);

            if (response.Status.Equals("error"))
            {
                logger.LogWarning("User login failed for email: {Email}.", authLogin.Email);
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("User login successful for email: {Email}", authLogin.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in logging in user for email: {Email}", authLogin.Email);
            return Problem("An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Blacklist refresh token of the authenticated user.
    /// </summary>
    /// <remarks>
    /// This endpoint invalidates the refresh token associated with the logged-in user,
    /// effectively logging them out of the application.
    /// </remarks>
    /// <param name="refreshToken">The user's refresh token used for authentication.</param>
    /// <returns>NoContent if successful, Unauthorized if user is not authenticated, 
    /// BadRequest if the refresh token is invalid or not found.</returns>
    /// <response code="204">No Content - Logout successful.</response>
    /// <response code="400">Bad Request - Invalid refresh token.</response>
    /// <response code="401">Unauthorized - User not authenticated.</response>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutUser([FromBody] string refreshToken)
    {
        try
        {
            var userId = ControllerUtil.GetUserId(User);
            logger.LogInformation("User logout attempt for userId: {UserId}", userId);

            if (userId == -1)
                return Unauthorized();

            var response = await authService.LogoutUserAsync(refreshToken);

            if (!response)
            {
                logger.LogWarning("Logout failed for userId: {UserId}. Invalid refresh token.", userId);
                return BadRequest();
            }

            logger.LogInformation("User successfully logged out for userId: {UserId}", userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in logging out user.");
            return Problem("An error occurred while processing your request.");
        }
    }

    /// <summary>
    /// Refreshes the authenticated user's access and refresh tokens.
    /// </summary>
    /// <param name="refreshToken">The user's refresh token used for authentication.</param>
    /// <returns>
    /// A response containing the new access token and refresh token if the provided refresh token is valid.
    /// </returns>
    /// <response code="200">Returns the new access and refresh tokens.</response>
    /// <response code="400">Indicates that the provided refresh token is invalid or expired.</response>
    /// <response code="401">Indicates that the user is unauthorized to refresh tokens.</response>
    /// <response code="500">Indicates an internal server error occurred during token processing.</response>
    [Authorize]
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<AuthDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(ErrorResponseDto))]
    public async Task<IActionResult> RefreshUserTokens([FromBody] string refreshToken)
    {
        try
        {
            var userId = ControllerUtil.GetUserId(User);

            if (userId == -1)
                return Unauthorized();

            var response = await authService.RefreshUserTokensAsync(userId, refreshToken);

            if (response.Status.Equals("error"))
                return ControllerUtil.GetActionResultFromError(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in renewing jwt tokens.");
            return Problem("An error occurred while processing your request.");
        }
    }
}
