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
    /// <response code="404">Unauthorized access.</response>
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
        {
            return BadRequest(ControllerUtil.ValidateRequest<AuthDto>(ModelState));
        }

        try
        {
            var response = await authService.RegisterUserAsync(authRegister);

            if (response.Status.Equals("error"))
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
    ///     - <see cref="UnauthorizedObjectResult"/> if the user entered invalid credentials.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="200">Returns the access and refresh tokens.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("login")]
    [Consumes("application/json")]
    [Produces("application/json")]
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ControllerUtil.ValidateRequest<AuthDto>(ModelState));
            }

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
    ///     Blacklist refresh token of the authenticated user.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns> 
    ///     Returns an <see cref="IActionResult"/> containing:
    ///     - <see cref="NoContentResult"/>if the request is valid.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="204">No content.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("logout")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LogoutUser([FromBody][Required] string refreshToken)
    {
        try
        {
            logger.LogInformation("Attempting to Logout User.");
            var response = await authService.LogoutUserAsync(refreshToken);

            if (!response)
            {
                logger.LogWarning("Logout failed. Invalid refresh token.");
                return BadRequest();
            }

            logger.LogInformation("User successfully logged.");
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in logging out user.");
            return Problem("An error occurred while processing your request.");
        }
    }

    /// <summary>
    ///     Refreshes the authenticated user's tokens.
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns>
    ///     Returns an <see cref="IActionResult"/> containing:
    ///     - <see cref="OkObjectResult"/> if the request is valid.
    ///     - <see cref="UnauthorizedObjectResult"/> if an the credential is invalid.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="200">Returns the new access and refresh tokens.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK,
        Type = typeof(SuccessResponseDto<AuthDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(UnauthorizedResult))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshUserTokens([FromBody][Required] string refreshToken)
    {
        try
        {
            logger.LogInformation("Attempting to Refresh User Token.");

            var response = await authService.RefreshUserTokensAsync(refreshToken);
            if (response.Status.Equals("error"))
            {
                logger.LogWarning("Failed to refresh user token.");
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("Successfully refreshed user tokens.");
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in renewing jwt tokens.");
            return Problem("An error occurred while processing your request.");
        }
    }
}
