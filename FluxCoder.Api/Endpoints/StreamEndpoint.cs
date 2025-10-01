using FluxCoder.Api.Data;
using FluxCoder.Api.DTOs.Stream;
using Microsoft.EntityFrameworkCore;

namespace FluxCoder.Api.Endpoints;

public static class StreamEndpoint
{
    public static IEndpointRouteBuilder MapStreamEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/streams")
            .WithTags("Видео-потоки")
            .RequireAuthorization()
            .WithOpenApi();
        
        group.MapGet("/", GetStreamsAsync)
            .RequireAuthorization()
            .WithName("GetStreams")
            .WithSummary("Получить все видео потоки")
            .WithTags("Streams");

        group.MapGet("/{id:int}", GetStreamById)
            .RequireAuthorization()
            .WithSummary("Получить видео поток по его Id")
            .WithName("GetStreamById")
            .WithTags("Streams");
        
        return group;
    }

    private static async Task<IResult> GetStreamsAsync(AppDbContext db, CancellationToken ct = default)
    {
        var streams = await db.Streams
            .Select(s => new StreamResponse(
                s.Id,
                s.Name,
                s.InputUrl,
                s.OutputUrl,
                s.Status,
                s.FFmpegArguments,
                s.CreatedAt,
                s.LastStartedAt))
            .ToListAsync(ct);
        
        return Results.Ok(streams);
    }

    private static async Task<IResult> GetStreamById(int id, AppDbContext db, CancellationToken ct = default)
    {
        var stream = await db.Streams.FirstOrDefaultAsync(x => x.Id.Equals(id), ct);

        if (stream is null)
            return Results.NotFound();
        
        return Results.Ok(new StreamResponse(
            stream.Id,
            stream.Name,
            stream.InputUrl,
            stream.OutputUrl,
            stream.Status,
            stream.FFmpegArguments,
            stream.CreatedAt,
            stream.LastStartedAt
        ));
        
    }
}