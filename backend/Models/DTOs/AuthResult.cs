using Backend.Models;

namespace Backend.Models.DTOs;

public class AuthResult
{
    public bool Success { get; init; }
    public string Detail { get; init; } = string.Empty;
    public AuthResponse? AuthResponse { get; init; }
}