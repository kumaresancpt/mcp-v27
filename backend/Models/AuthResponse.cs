namespace Backend.Models;

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Role,
    string UserId,
    string SessionId);