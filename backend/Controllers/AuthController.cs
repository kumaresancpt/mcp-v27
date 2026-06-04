using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { detail = "Invalid login request." });
        }

        var result = await _authService.AuthenticateAsync(request);
        if (!result.Success || result.AuthResponse is null)
        {
            return BadRequest(new { detail = result.Detail });
        }

        Response.Cookies.Append("accessToken", result.AuthResponse.AccessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Expires = result.AuthResponse.ExpiresAtUtc
        });

        return Ok(result.AuthResponse);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { detail = "Invalid forgot password request." });
        }

        var response = await _authService.RequestPasswordResetAsync(request);
        return Ok(response);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { detail = "Invalid reset password request." });
        }

        var result = await _authService.ResetPasswordAsync(request);
        if (!result.Success)
        {
            return BadRequest(new { detail = result.Detail });
        }

        return Ok(new ResetPasswordResponse("Password has been reset successfully."));
    }
}