namespace FluxCoder.Api.Models;

public class Stream
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InputUrl { get; set; } = string.Empty; 
    public string OutputUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Stopped"; 
    public string? FFmpegArguments { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastStartedAt { get; set; }
    public int CreatedByUserId { get; set; }
    
    // Navigation
    public User? CreatedBy { get; set; }
}