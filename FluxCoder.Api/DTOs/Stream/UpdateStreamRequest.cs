namespace FluxCoder.Api.DTOs.Stream;

public record UpdateStreamRequest(
    string Name,
    string InputUrl,
    string VideoCodec,
    string AudioCodec,
    string Quality,
    int VideoBitrate,
    int AudioBitrate);