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
        try
        {
            logger.LogInformation("User registration attempt for email: {Email}", authRegister.Email);
            if (!ModelState.IsValid)
            {
                return BadRequest(ControllerUtil.ValidateRequest<AuthDto>(ModelState));
            }

            var response = await authService.RegisterUserAsync(authRegister);

            if (response.Status.Equals("error"))
            {
                logger.LogWarning("User registration failed for email: {Email}.", authRegister.Email);
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("User registration successful for email: {Email}", authRegister.Email);
            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in registering user details for email: {Email}", authRegister.Email);
            return Problem("An error occurred while processing your request.");
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
    ///     - <see cref="UnauthorizedObjectResult"/> if an the credential is invalid.
    ///     - <see cref="ProblemDetails"/> if an internal server error occurs.
    /// </returns>
    /// <response code="204">No content.</response>
    /// <response code="401">Unauthorized access.</response>
    /// <response code="500">Internal server error.</response>
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
    [ProducesResponseType(StatusCodes.Status401Unauthorized,
        Type = typeof(ErrorResponseDto))]
    public async Task<IActionResult> RefreshUserTokens([FromBody] string refreshToken)
    {
        try
        {
            var userId = ControllerUtil.GetUserId(User);
            logger.LogInformation("User token refresh attempt for userId: {UserId}", userId);

            if (userId == -1)
                return Unauthorized();

            var response = await authService.RefreshUserTokensAsync(refreshToken);
            if (response.Status.Equals("error"))
            {
                logger.LogWarning("Token refresh failed for userId: {UserId}.", userId);
                return ControllerUtil.GetActionResultFromError(response);
            }

            logger.LogInformation("Tokens refreshed successfully for userId: {UserId}", userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Error in renewing jwt tokens.");
            return Problem("An error occurred while processing your request.");
        }
    }
}
