using Microsoft.AspNetCore.Mvc;
using SecureEmployeeManagement.DTOs;
using SecureEmployeeManagement.Interfaces;

namespace SecureEmployeeManagement.Controllers;

[ApiController]
[Route("api/[controller]")]   // → /api/auth
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("register")]     // POST /api/auth/register
    public IActionResult Register([FromBody] RegisterDto dto)
    {
        var result = _authService.Register(dto);
        if (result is null)
            return Conflict(new { message = "Username or email already taken." });

        return Ok(result);
    }

    [HttpPost("login")]        // POST /api/auth/login
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var result = _authService.Login(dto);
        if (result is null)
            return Unauthorized(new { message = "Invalid credentials." });   // 401

        return Ok(result);
    }
}