using System.Collections.Concurrent;
using System.Security.Cryptography;
using Backend.Models.DTOs;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class PasswordResetService : IPasswordResetService
{
    private readonly PasswordResetSettings _settings;
    private readonly ConcurrentDictionary<string, List<ResetTokenState>> _tokens = new(StringComparer.OrdinalIgnoreCase);

    public PasswordResetService(IOptions<PasswordResetSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<string> CreateResetTokenAsync(string userKey)
    {
        var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
        var tokenState = new ResetTokenState
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(_settings.TokenExpiryMinutes),
            IsUsed = false
        };

        var userTokens = _tokens.GetOrAdd(userKey, _ => new List<ResetTokenState>());
        lock (userTokens)
        {
            userTokens.Add(tokenState);
        }

        return Task.FromResult(token);
    }

    public Task<bool> ValidateTokenAsync(string userKey, string token)
    {
        if (!_tokens.TryGetValue(userKey, out var userTokens))
        {
            return Task.FromResult(false);
        }

        lock (userTokens)
        {
            var match = userTokens.LastOrDefault(item => item.Token == token);
            if (match is null)
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(!match.IsUsed && match.ExpiresAtUtc > DateTime.UtcNow);
        }
    }

    public Task ConsumeTokenAsync(string userKey, string token)
    {
        if (!_tokens.TryGetValue(userKey, out var userTokens))
        {
            return Task.CompletedTask;
        }

        lock (userTokens)
        {
            var match = userTokens.LastOrDefault(item => item.Token == token);
            if (match is not null)
            {
                match.IsUsed = true;
            }
        }

        return Task.CompletedTask;
    }

    private sealed class ResetTokenState
    {
        public required string Token { get; init; }
        public DateTime ExpiresAtUtc { get; init; }
        public bool IsUsed { get; set; }
    }
}