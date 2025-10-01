using System.Security.Claims;
using FluxCoder.Api.Data;
using FluxCoder.Api.DTOs.Stream;
using FluxCoder.Api.Services;
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
        
        group.MapPost("/", CreateStream)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Создать видео поток")
            .WithName("CreateStream")
            .WithTags("Streams");
        
        group.MapPut("/", UpdateStream)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Обновить видео поток")
            .WithName("UpdateStream")
            .WithTags("Streams");
        
        group.MapDelete("/{id:int}", DeleteStream)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Удалить видео поток")
            .WithName("DeleteStream")
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

    private static async Task<IResult> CreateStream(
        CreateStreamRequest request,
        StreamService streamService,
        ClaimsPrincipal  user,
        CancellationToken ct = default)
    {
        var userId = UserContextService.GetUserId(user);
        var stream = await streamService.CreateStreamAsync(request, userId);
        
        var response = new StreamResponse(
            stream.Id,
            stream.Name,
            stream.InputUrl,
            stream.OutputUrl,
            stream.Status,
            stream.FFmpegArguments,
            stream.CreatedAt,
            stream.LastStartedAt
        );

        return Results.Created($"/api/streams/{stream.Id}", response);
    }

    private static async Task<IResult> UpdateStream(
        int id,
        UpdateStreamRequest request,
        StreamService streamService,
        CancellationToken ct = default)
    {
        var stream = await streamService.UpdateStreamAsync(id, request, ct);

        if (stream is null)
            return Results.NotFound(new { message = "Видео поток не найден" });
        
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

    private static async Task<IResult> DeleteStream(int id, StreamService streamService, CancellationToken ct = default)
    {
        var deleted = await streamService.DeleteStreamAsync(id, ct);

        return !deleted
            ? Results.NotFound(new { message = "Видео поток не найден" }) 
            : Results.Ok(new { message = "Видео поток удален" });
    }
}