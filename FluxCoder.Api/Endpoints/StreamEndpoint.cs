using System.Security.Claims;
using FluxCoder.Api.Common.Filters;
using FluxCoder.Api.Data;
using FluxCoder.Api.DTOs.Stream;
using FluxCoder.Api.Extensions;
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
            .WithOpenApi()
            .AddEndpointFilter<RequestLoggingFilter>();
        
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
            .WithTags("Streams")
            .WithRequestValidation<CreateStreamRequest>();
        
        group.MapPut("/", UpdateStream)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Обновить видео поток")
            .WithName("UpdateStream")
            .WithTags("Streams")
            .WithRequestValidation<UpdateStreamRequest>();
        
        group.MapDelete("/{id:int}", DeleteStream)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Удалить видео поток")
            .WithName("DeleteStream")
            .WithTags("Streams");
        
        return group;
    }

    private static async Task<IResult> GetStreamsAsync(StreamService streamService, CancellationToken ct = default)
    {
        var streams = await streamService.GetAllStreamsAsync(ct);
    
        var response = streams.Select(s => new StreamResponse(
            s.Id,
            s.Name,
            s.InputUrl,
            s.StreamKey,
            s.VideoCodec,
            s.AudioCodec,
            s.Quality,
            s.VideoBitrate,
            s.AudioBitrate,
            s.Status,
            streamService.GetHlsUrl(s),
            s.CreatedAt,
            s.LastStartedAt
        ));

        return Results.Ok(response);
    }

    private static async Task<IResult> GetStreamById(int id, StreamService streamService, CancellationToken ct = default)
    {
        var stream = await streamService.GetStreamByIdAsync(id, ct);
    
        if (stream == null)
            return Results.NotFound(new { message = "Видео поток не найден" });

        var response = new StreamResponse(
            stream.Id,
            stream.Name,
            stream.InputUrl,
            stream.StreamKey,
            stream.VideoCodec,
            stream.AudioCodec,
            stream.Quality,
            stream.VideoBitrate,
            stream.AudioBitrate,
            stream.Status,
            streamService.GetHlsUrl(stream),
            stream.CreatedAt,
            stream.LastStartedAt
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> CreateStream(
        CreateStreamRequest request,
        StreamService streamService,
        ClaimsPrincipal  user,
        CancellationToken ct = default)
    {
        var existing = await streamService.GetStreamByKeyAsync(request.StreamKey, ct);
        if (existing != null)
        {
            return Results.BadRequest(new { message = "Ключ потока уже существует" });
        }

        var userId = UserContextService.GetUserId(user);
        var stream = await streamService.CreateStreamAsync(request, userId, ct);

        var response = new StreamResponse(
            stream.Id,
            stream.Name,
            stream.InputUrl,
            stream.StreamKey,
            stream.VideoCodec,
            stream.AudioCodec,
            stream.Quality,
            stream.VideoBitrate,
            stream.AudioBitrate,
            stream.Status,
            streamService.GetHlsUrl(stream),
            stream.CreatedAt,
            stream.LastStartedAt
        );

        return Results.Created($"/api/streams/{stream.Id}", response);
    }

    private static async Task<IResult> UpdateStream(
        int id,
        UpdateStreamRequest request,
        StreamService streamService,
        FFmpegService ffmpegService,
        CancellationToken ct = default)
    {
        if (ffmpegService.IsStreamRunning(id))
        {
            return Results.BadRequest(new { message = "Невозможно обновить текущий видео поток. Сначала остановите его." });
        }

        var stream = await streamService.UpdateStreamAsync(id, request, ct);
    
        if (stream == null)
            return Results.NotFound(new { message = "Stream not found" });

        var response = new StreamResponse(
            stream.Id,
            stream.Name,
            stream.InputUrl,
            stream.StreamKey,
            stream.VideoCodec,
            stream.AudioCodec,
            stream.Quality,
            stream.VideoBitrate,
            stream.AudioBitrate,
            stream.Status,
            streamService.GetHlsUrl(stream),
            stream.CreatedAt,
            stream.LastStartedAt
        );

        return Results.Ok(response);
    }

    private static async Task<IResult> DeleteStream(
        int id, 
        StreamService streamService,
        FFmpegService ffmpegService,
        CancellationToken ct = default)
    {
        if (ffmpegService.IsStreamRunning(id))
        {
            ffmpegService.StopStream(id);
        }
        
        var deleted = await streamService.DeleteStreamAsync(id, ct);

        return !deleted
            ? Results.NotFound(new { message = "Видео поток не найден" }) 
            : Results.Ok(new { message = "Видео поток удален" });
    }
}