using FluxCoder.Api.Data;
using FluxCoder.Api.DTOs.Stream;
using Microsoft.EntityFrameworkCore;
using Stream = FluxCoder.Api.Models.Stream;

namespace FluxCoder.Api.Services;

public class StreamService
{
    private readonly AppDbContext _db;

    public StreamService(AppDbContext db)
    {
        _db = db;
    }
    
    public async Task<Stream> CreateStreamAsync(CreateStreamRequest request, int createdByUserId)
    {
        var stream = new Stream()
        {
            Name = request.Name,
            InputUrl = request.InputUrl,
            OutputUrl = request.OutputUrl,
            FFmpegArguments = request.FFmpegArguments,
            Status = "Stopped",
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Streams.Add(stream);
        await _db.SaveChangesAsync();

        return stream;
    }
    
    public async Task<Stream?> UpdateStreamAsync(int id, UpdateStreamRequest request)
    {
        var stream = await _db.Streams.FindAsync(id);
        
        if (stream == null)
            return null;

        stream.Name = request.Name;
        stream.InputUrl = request.InputUrl;
        stream.OutputUrl = request.OutputUrl;
        stream.FFmpegArguments = request.FFmpegArguments;

        await _db.SaveChangesAsync();

        return stream;
    }

    public async Task<bool> DeleteStreamAsync(int id)
    {
        var stream = await _db.Streams.FindAsync(id);
        
        if (stream == null)
            return false;

        _db.Streams.Remove(stream);
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<Stream?> GetStreamByIdAsync(int id)
    {
        return await _db.Streams.FindAsync(id);
    }

    public async Task<List<Stream>> GetAllStreamsAsync()
    {
        return await _db.Streams.ToListAsync();
    }
    
    public async Task<Stream?> UpdateStreamStatusAsync(int id, string status)
    {
        var stream = await _db.Streams.FindAsync(id);
        
        if (stream == null)
            return null;

        stream.Status = status;
        
        if (status == "Running")
            stream.LastStartedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return stream;
    }
}