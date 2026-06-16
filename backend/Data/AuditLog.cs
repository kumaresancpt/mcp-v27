namespace Backend.Data;

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Actor { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime EventTimestampUtc { get; set; } = DateTime.UtcNow;
    public string MetadataJson { get; set; } = "{}";
    public string PreviousHash { get; set; } = "GENESIS";
    public string Hash { get; set; } = string.Empty;
}