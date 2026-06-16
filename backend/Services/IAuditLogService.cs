namespace Backend.Services;

public interface IAuditLogService
{
    Task RecordEventAsync(string actor, string eventType, IDictionary<string, string?> metadata);
}