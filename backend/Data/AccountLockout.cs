namespace Backend.Data;

public class AccountLockout
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public int FailedAttemptCount { get; set; }
    public DateTime? LockedUntilUtc { get; set; }
    public DateTime? LastFailedAtUtc { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}