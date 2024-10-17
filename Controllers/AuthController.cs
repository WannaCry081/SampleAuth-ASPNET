using Microsoft.AspNetCore.Mvc;

namespace sample_auth_aspnet.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(ILogger<AuthController> logger) : ControllerBase
{

}