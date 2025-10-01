namespace FluxCoder.Api.DTOs.Stream;

public record CreateStreamRequest(
    string Name,
    string InputUrl,
    string OutputUrl,
    string? FFmpegArguments);