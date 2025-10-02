namespace FluxCoder.Api.DTOs.Stream;

public record StreamResponse(
    int Id,
    string Name,
    string InputUrl,
    string StreamKey,
    string VideoCodec,
    string AudioCodec,
    string Quality,
    int VideoBitrate,
    int AudioBitrate,
    string Status,
    string HlsUrl, 
    DateTime CreatedAt,
    DateTime? LastStartedAt);