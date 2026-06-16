namespace Backend.Services;

public interface IPasswordResetService
{
    Task<string> CreateResetTokenAsync(string userKey);
    Task<bool> ValidateTokenAsync(string userKey, string token);
    Task ConsumeTokenAsync(string userKey, string token);
}