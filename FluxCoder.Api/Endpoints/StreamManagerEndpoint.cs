using FluxCoder.Api.DTOs.Stream;
using FluxCoder.Api.Services;

namespace FluxCoder.Api.Endpoints;

public static class StreamManagerEndpoint
{
    public static IEndpointRouteBuilder MapStreamManagerEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/streams")
            .WithTags("Управление видео-потоками")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapPost("/{id:int}/control", ControlStream)
            .RequireAuthorization() 
            .WithSummary("Управление транскодирование видео потоков")
            .WithName("ControlStream")
            .WithTags("Streams");
        
        return group;
    }

    private static async Task<IResult> ControlStream(
        int id, 
        StreamControlRequest request,
        StreamService streamService,
        CancellationToken cancellationToken = default)
    {
        var stream = await streamService.GetStreamByIdAsync(id, cancellationToken);

        if (stream is null)
            return Results.NotFound(new { message = "Видео поток не найден" });

        var validActions = new[] { "start", "stop", "pause", "resume" };

        if (!validActions.Contains(request.Action.ToLower()))
            return Results.BadRequest(new
                { message = "Неправильное действие выбрано\n Используйте: start, stop, pause или resume" });
        
        string newStatus;
        string message;
        
        switch (request.Action.ToLower())
        {
            case "start":
                if (stream.Status == "Running")
                    return Results.BadRequest(new { message = "Видео поток уже был запущен" });
            
                newStatus = "Running";
                message = "Транскодирование видео потока запущено успешно";
                // TODO Phase 2: здесь будем запускать FFmpeg процесс
                break;

            case "stop":
                if (stream.Status == "Stopped")
                    return Results.BadRequest(new { message = "Видео поток уже был остановлен" });
            
                newStatus = "Stopped";
                message = "Транскодирование видео потока остановлено";
                // TODO Phase 2: здесь будем останавливать FFmpeg процесс
                break;

            case "pause":
                if (stream.Status != "Running")
                    return Results.BadRequest(new { message = "Транскодирование видео потока должен быть запущен, чтобы поставить его на паузу" });
            
                newStatus = "Paused";
                message = "Транскодирование видео потока приостановлено";
                // TODO Phase 2: здесь будем ставить на паузу FFmpeg процесс
                break;

            case "resume":
                if (stream.Status != "Paused")
                    return Results.BadRequest(new { message = "Транскодирование видео потока должен быть приостановлено, чтобы его возобновить" });
            
                newStatus = "Running";
                message = "Транскодирование видео потока возобновлен успешно";
                // TODO Phase 2: здесь будем возобновлять FFmpeg процесс
                break;

            default:
                return Results.BadRequest(new { message = "Неизвестное действие" });
        }
        
        var updatedStream = await streamService.UpdateStreamStatusAsync(id, newStatus, cancellationToken);
        
        return Results.Ok();
    }
    
}