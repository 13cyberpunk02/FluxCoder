namespace FluxCoder.Api.Models.Settings;

public class HlsSettings
{
    public string OutputDirectory { get; set; } = "./hls-output"; 
    public int SegmentDuration { get; set; } = 6; 
    public int PlaylistSize { get; set; } = 5; 
    public string BaseUrl { get; set; } = "https://localhost:5443"; 
}