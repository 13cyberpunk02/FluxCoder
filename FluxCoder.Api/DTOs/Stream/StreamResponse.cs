namespace FluxCoder.Api.DTOs.Stream;

public record StreamResponse(
    int Id,
    string Name,
    string InputUrl,
    string OutputUrl,
    string Status,
    string? FFmpegArguments,
    DateTime CreatedAt,
    DateTime? LastStartedAt);