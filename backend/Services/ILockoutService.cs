namespace Backend.Services;

public interface ILockoutService
{
    Task<(bool IsLocked, DateTime? LockedUntilUtc)> GetLockoutStateAsync(string userKey);
    Task RecordFailedAttemptAsync(string userKey);
    Task ResetFailedAttemptsAsync(string userKey);
}