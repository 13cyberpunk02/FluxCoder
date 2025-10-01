namespace FluxCoder.Api.DTOs.Stream;

public record UpdateStreamRequest(
    string Name,
    string InputUrl,
    string OutputUrl,
    string? FFmpegArguments);