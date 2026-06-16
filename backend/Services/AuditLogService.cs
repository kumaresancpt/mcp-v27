using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;
    private readonly ConcurrentQueue<AuditEvent> _events = new();
    private string _lastHash = "GENESIS";

    public AuditLogService(ILogger<AuditLogService> logger)
    {
        _logger = logger;
    }

    public Task RecordEventAsync(string actor, string eventType, IDictionary<string, string?> metadata)
    {
        var timestamp = DateTime.UtcNow;
        var metadataSerialized = string.Join(';', metadata.OrderBy(item => item.Key).Select(item => $"{item.Key}={item.Value}"));
        var material = $"{_lastHash}|{timestamp:O}|{actor}|{eventType}|{metadataSerialized}";
        var hash = ComputeHash(material);

        var auditEvent = new AuditEvent(actor, eventType, timestamp, metadata, _lastHash, hash);
        _events.Enqueue(auditEvent);
        _lastHash = hash;

        _logger.LogInformation("AuditEvent: {EventType} actor={Actor} at={Timestamp}", eventType, actor, timestamp);
        return Task.CompletedTask;
    }

    private static string ComputeHash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    private sealed record AuditEvent(
        string Actor,
        string EventType,
        DateTime TimestampUtc,
        IDictionary<string, string?> Metadata,
        string PreviousHash,
        string Hash);
}