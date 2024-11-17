using Microsoft.AspNetCore.Mvc;
using sample_auth_aspnet.Controllers.Utils;
using sample_auth_aspnet.Models.Dtos.Auth;
using sample_auth_aspnet.Models.Dtos.Reponse;
using sample_auth_aspnet.Services.Auth;

namespace sample_auth_aspnet.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(
    ILogger<AuthController> logger,
    IAuthService authService) : ControllerBase
{
    /// <summary>
    ///     Registers a new user.
    /// </summary>
    /// <param name="authRegister"></param>
    /// <returns>
    ///     Returns an <see cref="IActionResult"/> containing:
    ///     - <see cref="StatusCodeResult" /> with the access and refresh tokens.
    ///     - <see cref="BadRequestObjectResult"/> if the request is invalid.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="201">Returns the access and refresh tokens.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("register")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status201Created,
        Type = typeof(SuccessResponseDto<AuthDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
        Type = typeof(ErrorResponseDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterUser([FromBody] AuthRegisterDto authRegister)
    {
        logger.LogInformation("User registration attempt initiated.");
        if (!ModelState.IsValid)
            return BadRequest(ControllerUtil.ValidateRequest<object>(ModelState));

        try
        {
            var response = await authService.RegisterUserAsync(authRegister);

            if (!response.Success)
            {
                logger.LogWarning("Registration failed for email: {Email}.", authRegister.Email);
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("Registration successful for email: {Email}", authRegister.Email);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred during user registration.");
            return Problem("An internal server error occurred. Please try again later.");
        }
    }

    /// <summary>
    ///     Authenticates registered user.
    /// </summary>
    /// <param name="authLogin"></param>
    /// <returns>
    ///     Returns an <see cref="IActionResult"/> containing:
    ///     - <see cref="OkObjectResult"/> with the access and refresh tokens.
    ///     - <see cref="BadRequestObjectResult"/> if the request is invalid. 
    ///     - <see cref="UnauthorizedObjectResult"/> if the user entered invalid credentials.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="200">Returns the access and refresh tokens.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<AuthDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
        Type = typeof(ErrorResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(ErrorResponseDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginUser([FromBody] AuthLoginDto authLogin)
    {
        logger.LogInformation("Login attempt for user.");
        if (!ModelState.IsValid)
            return BadRequest(ControllerUtil.ValidateRequest<object>(ModelState));

        try
        {
            var response = await authService.LoginUserAsync(authLogin);

            if (!response.Success)
            {
                logger.LogWarning("Login failed for email: {Email}.", authLogin.Email);
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("Login successful for email: {Email}", authLogin.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred during login.");
            return Problem("An internal server error occurred. Please try again later.");
        }
    }

    /// <summary>
    ///     Blacklist refresh token of the authenticated user.
    /// </summary>
    /// <param name="authRefreshToken"></param>
    /// <returns> 
    ///     Returns an <see cref="IActionResult"/> containing:
    ///     - <see cref="NoContentResult"/> if the request is valid.
    ///     - <see cref="BadRequestObjectResult"/> if the request is invalid.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="204">No content.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("logout")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LogoutUser([FromBody] AuthRefreshTokenDto authRefreshToken)
    {
        logger.LogInformation("Logout attempt for user.");
        if (!ModelState.IsValid)
            return BadRequest(ControllerUtil.ValidateRequest<object>(ModelState));

        try
        {
            var isSuccess = await authService.LogoutUserAsync(authRefreshToken.Refresh);

            if (!isSuccess)
            {
                logger.LogWarning("Logout failed. Invalid refresh token provided.");
                return BadRequest(new
                {
                    Message = "Invalid refresh token."
                });
            }

            logger.LogInformation("User logged out successfully.");
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred during logout.");
            return Problem("An internal server error occurred. Please try again later.");
        }
    }

    /// <summary>
    ///     Refreshes the authenticated user's tokens.
    /// </summary>
    /// <param name="authRefreshToken"></param>
    /// <returns>
    ///     Returns an <see cref="IActionResult"/> containing:
    ///     - <see cref="OkObjectResult"/> if the request is valid.
    ///     - <see cref="BadRequestObjectResult"/> if the request is invalid.
    ///     - <see cref="UnauthorizedObjectResult"/> if an the credential is invalid.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="200">Returns the new access and refresh tokens.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<AuthDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshUserTokens([FromBody] AuthRefreshTokenDto authRefreshToken)
    {
        logger.LogInformation("Token refresh attempt for user.");
        if (!ModelState.IsValid)
            return BadRequest(ControllerUtil.ValidateRequest<object>(ModelState));

        try
        {
            var response = await authService.RefreshUserTokensAsync(authRefreshToken.Refresh);

            if (!response.Success)
            {
                logger.LogWarning("Token refresh failed. Invalid refresh token.");
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("Tokens refreshed successfully.");
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred during token refresh.");
            return Problem("An internal server error occurred. Please try again later.");
        }
    }

    /// <summary>
    ///     Send an email containing the reset password link.
    /// </summary>
    /// <param name="authForgotPassword"></param>
    /// <returns>
    ///     Returns an <see cref="IActionResult" /> containing:
    ///     - <see cref="OkObjectResult"/> if the request is valid.
    ///     - <see cref="BadRequestObjectResult"/> if the request is invalid.
    ///     - <see cref="NotFoundObjectResult"/> if the request do not exists.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="200">Returns the new access and refresh tokens.</response>
    /// <response code="400">Bad request.</response>
    /// <response code="404">No resource found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("forgot-password")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<object?>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest,
        Type = typeof(ErrorResponseDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound,
        Type = typeof(ErrorResponseDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotUserPassword([FromBody] AuthForgotPasswordDto authForgotPassword)
    {
        logger.LogInformation("Forgot password request initiated for user.");

        if (!ModelState.IsValid)
            return BadRequest(ControllerUtil.ValidateRequest<object>(ModelState));

        try
        {
            var response = await authService.ForgotUserPasswordAsync(authForgotPassword.Email);

            if (!response.Success)
            {
                logger.LogWarning("Failed to send email to {Email}. The email might not be registered.", authForgotPassword.Email);
                return BadRequest(new
                {
                    Message = "Failed to process the forgot password request."
                });
            }

            logger.LogInformation("Forgot password email sent successfully to {Email}.", authForgotPassword.Email);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred during forgot password request for {Email}.", authForgotPassword.Email);
            return Problem("An internal server error occurred. Please try again later.");
        }
    }
}