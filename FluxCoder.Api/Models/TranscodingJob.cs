namespace FluxCoder.Api.Models;

public class TranscodingJob
{
    public int Id { get; set; }
    public int StreamId { get; set; }
    public int? ProcessId { get; set; } 
    public string Status { get; set; } = "Pending"; 
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StoppedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Logs { get; set; }
    
    public Stream? Stream { get; set; }
}