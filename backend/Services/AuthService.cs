using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Backend.Models.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILockoutService _lockoutService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IAuditLogService _auditLogService;
    private static readonly ConcurrentDictionary<string, SessionInfo> Sessions = new(StringComparer.OrdinalIgnoreCase);

    private readonly ConcurrentDictionary<string, UserRecord> _users = new(StringComparer.OrdinalIgnoreCase);

    public AuthService(
        IConfiguration configuration,
        ILockoutService lockoutService,
        IPasswordResetService passwordResetService,
        IAuditLogService auditLogService)
    {
        _configuration = configuration;
        _lockoutService = lockoutService;
        _passwordResetService = passwordResetService;
        _auditLogService = auditLogService;

        SeedUsers();
    }

    public async Task<AuthResult> AuthenticateAsync(LoginRequest request)
    {
        var userKey = BuildUserKey(request.Username, request.Role);
        var (isLocked, lockedUntilUtc) = await _lockoutService.GetLockoutStateAsync(userKey);
        if (isLocked)
        {
            await _auditLogService.RecordEventAsync(request.Username, "auth.lockout.blocked", new Dictionary<string, string?>
            {
                ["role"] = request.Role,
                ["lockedUntilUtc"] = lockedUntilUtc?.ToString("O")
            });

            return new AuthResult
            {
                Success = false,
                Detail = $"Account is locked until {lockedUntilUtc:O}."
            };
        }

        if (!_users.TryGetValue(userKey, out var user) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            await _lockoutService.RecordFailedAttemptAsync(userKey);
            var (nowLocked, nowLockedUntilUtc) = await _lockoutService.GetLockoutStateAsync(userKey);

            await _auditLogService.RecordEventAsync(request.Username, nowLocked ? "auth.lockout.triggered" : "auth.login.failed", new Dictionary<string, string?>
            {
                ["role"] = request.Role,
                ["locked"] = nowLocked.ToString(),
                ["lockedUntilUtc"] = nowLockedUntilUtc?.ToString("O")
            });

            return new AuthResult
            {
                Success = false,
                Detail = nowLocked
                    ? $"Account is locked until {nowLockedUntilUtc:O}."
                    : "Invalid username, role, or password."
            };
        }

        await _lockoutService.ResetFailedAttemptsAsync(userKey);

        var token = GenerateToken(user.UserId, user.Username, user.Role);
        var expiryMinutes = _configuration.GetValue<int?>("JwtSettings:ExpiryMinutes") ?? 60;
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var sessionId = Guid.NewGuid().ToString("N");
        Sessions[sessionId] = new SessionInfo
        {
            SessionId = sessionId,
            UserId = user.UserId,
            Username = user.Username,
            Role = user.Role,
            ExpiresAtUtc = expiresAtUtc
        };

        await _auditLogService.RecordEventAsync(user.Username, "auth.login.success", new Dictionary<string, string?>
        {
            ["role"] = user.Role,
            ["sessionId"] = sessionId
        });

        return new AuthResult
        {
            Success = true,
            Detail = "Authenticated successfully.",
            AuthResponse = new AuthResponse(token, expiresAtUtc, user.Role, user.UserId, sessionId)
        };
    }

    public async Task<ForgotPasswordResponse> RequestPasswordResetAsync(ForgotPasswordRequest request)
    {
        var userKey = BuildUserKey(request.Username, request.Role);

        if (_users.ContainsKey(userKey))
        {
            var token = await _passwordResetService.CreateResetTokenAsync(userKey);
            await _auditLogService.RecordEventAsync(request.Username, "auth.password.reset.requested", new Dictionary<string, string?>
            {
                ["role"] = request.Role,
                ["tokenIssued"] = (!string.IsNullOrWhiteSpace(token)).ToString()
            });
        }
        else
        {
            await _auditLogService.RecordEventAsync(request.Username, "auth.password.reset.requested.unknown-user", new Dictionary<string, string?>
            {
                ["role"] = request.Role
            });
        }

        return new ForgotPasswordResponse("If the account exists, a reset token has been issued.");
    }

    public async Task<ServiceOperationResult> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var userKey = BuildUserKey(request.Username, request.Role);
        if (!_users.TryGetValue(userKey, out var user))
        {
            await _auditLogService.RecordEventAsync(request.Username, "auth.password.reset.failed", new Dictionary<string, string?>
            {
                ["role"] = request.Role,
                ["reason"] = "unknown-user"
            });

            return new ServiceOperationResult
            {
                Success = false,
                Detail = "Invalid reset token or account details."
            };
        }

        var isValid = await _passwordResetService.ValidateTokenAsync(userKey, request.Token);
        if (!isValid)
        {
            await _auditLogService.RecordEventAsync(request.Username, "auth.password.reset.failed", new Dictionary<string, string?>
            {
                ["role"] = request.Role,
                ["reason"] = "invalid-or-expired-token"
            });

            return new ServiceOperationResult
            {
                Success = false,
                Detail = "Reset token is invalid or expired."
            };
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword, workFactor: 12);
        await _passwordResetService.ConsumeTokenAsync(userKey, request.Token);

        await _auditLogService.RecordEventAsync(request.Username, "auth.password.reset.success", new Dictionary<string, string?>
        {
            ["role"] = request.Role
        });

        return new ServiceOperationResult
        {
            Success = true,
            Detail = "Password reset successful."
        };
    }

    private string GenerateToken(string userId, string username, string role)
    {
        var secretKey = _configuration["JwtSettings:SecretKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecretKey not configured");
        var issuer = _configuration["JwtSettings:Issuer"]
            ?? throw new InvalidOperationException("JwtSettings:Issuer not configured");
        var expiryMinutes = _configuration.GetValue<int?>("JwtSettings:ExpiryMinutes") ?? 60;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.Role, role),
            new("role", role)
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            issuer,
            claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private void SeedUsers()
    {
        AddUser("admin@vms.local", "Admin", "u-admin-001", "Password@123");
        AddUser("reception@vms.local", "Receptionist", "u-reception-001", "Password@123");
        AddUser("guard@vms.local", "Security Guard", "u-guard-001", "Password@123");
    }

    private void AddUser(string username, string role, string userId, string password)
    {
        var userKey = BuildUserKey(username, role);
        _users[userKey] = new UserRecord
        {
            UserId = userId,
            Username = username,
            Role = role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12)
        };
    }

    private static string BuildUserKey(string username, string role) => $"{username.Trim().ToLowerInvariant()}::{role.Trim().ToLowerInvariant()}";

    private sealed class UserRecord
    {
        public required string UserId { get; init; }
        public required string Username { get; init; }
        public required string Role { get; init; }
        public string PasswordHash { get; set; } = string.Empty;
    }

    private sealed class SessionInfo
    {
        public required string SessionId { get; init; }
        public required string UserId { get; init; }
        public required string Username { get; init; }
        public required string Role { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
    }
}