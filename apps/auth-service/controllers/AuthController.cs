using Microsoft.AspNetCore.Mvc;
using AuthService.Services;
using Shared.Contracts.DTOs.Auth;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
   private readonly AuthServiceImpl _auth;

    public AuthController(AuthServiceImpl auth)
{
    _auth = auth;
}

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        await _auth.RegisterAsync(request);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);
        return Ok(result);
    }
}