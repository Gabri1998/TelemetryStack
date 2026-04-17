using Microsoft.AspNetCore.Mvc;
using ApiGateway.Services;
using Shared.Contracts.DTOs.Auth;

namespace ApiGateway.Controllers;  // FIXED: Changed from AuthService.Controllers to ApiGateway.Controllers

[ApiController]
[Route("api/auth")]  // Note: This has "api/" prefix
public class AuthController : ControllerBase
{
    private readonly AuthClient _client;  // FIXED: Using AuthClient, not AuthServiceImpl

    public AuthController(AuthClient client)
    {
        _client = client;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        try
        {
            await _client.RegisterAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        try
        {
            var result = await _client.LoginAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}