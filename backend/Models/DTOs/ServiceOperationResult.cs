namespace Backend.Models.DTOs;

public class ServiceOperationResult
{
    public bool Success { get; init; }
    public string Detail { get; init; } = string.Empty;
}