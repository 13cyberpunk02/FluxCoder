namespace FluxCoder.Api.DTOs.Stream;

public record CreateStreamRequest(
    string Name,
    string InputUrl,
    string StreamKey, 
    string VideoCodec = "libx264",
    string AudioCodec = "aac", 
    string Quality = "medium",
    int VideoBitrate = 2000,
    int AudioBitrate = 128);