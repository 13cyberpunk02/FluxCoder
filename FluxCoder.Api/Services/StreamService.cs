using FluxCoder.Api.Data;
using FluxCoder.Api.DTOs.Stream;
using FluxCoder.Api.Models.Settings;
using Microsoft.EntityFrameworkCore;
using Stream = FluxCoder.Api.Models.Stream;

namespace FluxCoder.Api.Services;

public class StreamService
{
    private readonly AppDbContext _db;
    private readonly HlsSettings _hlsSettings;

    public StreamService(AppDbContext db, HlsSettings hlsSettings)
    {
        _db = db;
        _hlsSettings = hlsSettings;
    }

    public async Task<Stream> CreateStreamAsync(CreateStreamRequest request, int createdByUserId, CancellationToken cancellationToken = default)
    {
        var stream = new Stream
        {
            Name = request.Name,
            InputUrl = request.InputUrl,
            StreamKey = request.StreamKey,
            VideoCodec = request.VideoCodec,
            AudioCodec = request.AudioCodec,
            Quality = request.Quality,
            VideoBitrate = request.VideoBitrate,
            AudioBitrate = request.AudioBitrate,
            Status = "Stopped",
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _db.Streams.AddAsync(stream, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return stream;
    }

    public async Task<Stream?> UpdateStreamAsync(int id, UpdateStreamRequest request, CancellationToken ct = default)
    {
        var stream = await _db.Streams.FirstOrDefaultAsync(x => x.Id == id, ct);
        
        if (stream == null)
            return null;

        stream.Name = request.Name;
        stream.InputUrl = request.InputUrl;
        stream.VideoCodec = request.VideoCodec;
        stream.AudioCodec = request.AudioCodec;
        stream.Quality = request.Quality;
        stream.VideoBitrate = request.VideoBitrate;
        stream.AudioBitrate = request.AudioBitrate;

        await _db.SaveChangesAsync(ct);

        return stream;
    }

    public async Task<bool> DeleteStreamAsync(int id, CancellationToken ct = default)
    {
        var stream = await _db.Streams.FirstOrDefaultAsync(x => x.Id == id, ct);
        
        if (stream == null)
            return false;

        _db.Streams.Remove(stream);
        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<Stream?> GetStreamByIdAsync(int id, CancellationToken ct = default)
        => await _db.Streams.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<List<Stream>> GetAllStreamsAsync(CancellationToken ct = default)
    {
        return await _db.Streams.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Stream?> UpdateStreamStatusAsync(int id, string status, CancellationToken ct = default)
    {
        var stream = await _db.Streams.FirstOrDefaultAsync(x => x.Id == id, ct);
        
        if (stream == null)
            return null;

        stream.Status = status;
        
        if (status == "Running")
            stream.LastStartedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return stream;
    }

    public async Task<Stream?> GetStreamByKeyAsync(string streamKey, CancellationToken ct = default)
    {
        return await _db.Streams.FirstOrDefaultAsync(s => s.StreamKey == streamKey, ct);
    }

    public string GetHlsUrl(Stream stream)
    {
        return stream.GetHlsUrl(_hlsSettings.BaseUrl);
    }
}