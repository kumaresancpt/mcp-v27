namespace Backend.Models.DTOs;

public class PasswordResetSettings
{
    public int TokenExpiryMinutes { get; set; } = 10;
}