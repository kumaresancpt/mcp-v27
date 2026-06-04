using System.Collections.Concurrent;
using Backend.Models.DTOs;
using Microsoft.Extensions.Options;

namespace Backend.Services;

public class LockoutService : ILockoutService
{
    private readonly LockoutSettings _settings;
    private readonly ConcurrentDictionary<string, LockoutState> _lockoutStates = new(StringComparer.OrdinalIgnoreCase);

    public LockoutService(IOptions<LockoutSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<(bool IsLocked, DateTime? LockedUntilUtc)> GetLockoutStateAsync(string userKey)
    {
        if (!_lockoutStates.TryGetValue(userKey, out var state))
        {
            return Task.FromResult((false, (DateTime?)null));
        }

        if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value > DateTime.UtcNow)
        {
            return Task.FromResult((true, state.LockedUntilUtc));
        }

        if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value <= DateTime.UtcNow)
        {
            state.FailedAttempts = 0;
            state.LockedUntilUtc = null;
        }

        return Task.FromResult((false, (DateTime?)null));
    }

    public Task RecordFailedAttemptAsync(string userKey)
    {
        var state = _lockoutStates.GetOrAdd(userKey, _ => new LockoutState());
        state.FailedAttempts++;

        if (state.FailedAttempts >= _settings.MaxFailedAttempts)
        {
            state.LockedUntilUtc = DateTime.UtcNow.AddMinutes(_settings.LockoutMinutes);
            state.FailedAttempts = 0;
        }

        return Task.CompletedTask;
    }

    public Task ResetFailedAttemptsAsync(string userKey)
    {
        if (_lockoutStates.TryGetValue(userKey, out var state))
        {
            state.FailedAttempts = 0;
            state.LockedUntilUtc = null;
        }

        return Task.CompletedTask;
    }

    private sealed class LockoutState
    {
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
    }
}