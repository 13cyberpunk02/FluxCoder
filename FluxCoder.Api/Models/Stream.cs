namespace FluxCoder.Api.Models;

public class Stream
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string InputUrl { get; set; } = string.Empty;
    public string StreamKey { get; set; } = string.Empty; 
    public string VideoCodec { get; set; } = "libx264"; 
    public string AudioCodec { get; set; } = "aac"; 
    public string Quality { get; set; } = "medium";
    public int VideoBitrate { get; set; } = 2000;
    public int AudioBitrate { get; set; } = 128; 
    public string Status { get; set; } = "Stopped";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastStartedAt { get; set; }
    public int CreatedByUserId { get; set; }
    
    public User? CreatedBy { get; set; }
    
    public string GetHlsUrl(string baseUrl)
    {
        return $"{baseUrl}/hls/{StreamKey}/playlist.m3u8";
    }
}