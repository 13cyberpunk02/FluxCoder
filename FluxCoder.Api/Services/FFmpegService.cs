using System.Diagnostics;
using FluxCoder.Api.Models.Settings;
using Stream = FluxCoder.Api.Models.Stream;
namespace FluxCoder.Api.Services;

public class FFmpegService
{
    private readonly HlsSettings _hlsSettings;
    private readonly Dictionary<int, Process> _activeProcesses = new();

    public FFmpegService(HlsSettings hlsSettings)
    {
        _hlsSettings = hlsSettings;
        
        if (!Directory.Exists(_hlsSettings.OutputDirectory))
        {
            Directory.CreateDirectory(_hlsSettings.OutputDirectory);
        }
    }

    public bool StartStream(Stream stream)
    {
        if (_activeProcesses.ContainsKey(stream.Id))
        {
            return false;
        }

        var streamDirectory = Path.Combine(_hlsSettings.OutputDirectory, stream.StreamKey);
        if (!Directory.Exists(streamDirectory))
        {
            Directory.CreateDirectory(streamDirectory);
        }

        var ffmpegArgs = BuildFFmpegCommand(stream, streamDirectory);

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Console.WriteLine($"[FFmpeg {stream.StreamKey}]: {args.Data}");
                }
            };

            process.Start();
            process.BeginErrorReadLine();

            _activeProcesses[stream.Id] = process;
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start FFmpeg for stream {stream.Id}: {ex.Message}");
            return false;
        }
    }

    public bool StopStream(int streamId)
    {
        if (!_activeProcesses.TryGetValue(streamId, out var process))
        {
            return false;
        }

        try
        {
            if (!process.HasExited)
            {
                process.StandardInput.WriteLine("q");
                
                if (!process.WaitForExit(5000))
                {
                    process.Kill();
                }
            }

            _activeProcesses.Remove(streamId);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to stop FFmpeg for stream {streamId}: {ex.Message}");
            return false;
        }
    }

    public bool IsStreamRunning(int streamId)
    {
        if (_activeProcesses.TryGetValue(streamId, out var process))
        {
            return !process.HasExited;
        }
        return false;
    }

    private string BuildFFmpegCommand(Stream stream, string outputDirectory)
    {
        var playlistPath = Path.Combine(outputDirectory, "playlist.m3u8");
        var segmentPath = Path.Combine(outputDirectory, "segment_%03d.ts");
        
        var args = new List<string>
        {
            "-i", stream.InputUrl, 
            "-y" 
        };
        
        args.AddRange(GetVideoCodecArgs(stream));
        
        args.AddRange(GetAudioCodecArgs(stream));
        
        args.AddRange([
            "-f", "hls",
            "-hls_time", _hlsSettings.SegmentDuration.ToString(),
            "-hls_list_size", _hlsSettings.PlaylistSize.ToString(),
            "-hls_flags", "delete_segments+append_list",
            "-hls_segment_filename", segmentPath,
            playlistPath
        ]);

        return string.Join(" ", args);
    }

    private static string[] GetVideoCodecArgs(Stream stream)
    {
        if (stream.VideoCodec == "copy")
        {
            return ["-c:v", "copy"];
        }

        var preset = stream.Quality switch
        {
            "low" => "ultrafast",
            "medium" => "fast",
            "high" => "medium",
            _ => "fast"
        };

        return
        [
            "-c:v", stream.VideoCodec,
            "-preset", preset,
            "-b:v", $"{stream.VideoBitrate}k",
            "-maxrate", $"{stream.VideoBitrate}k",
            "-bufsize", $"{stream.VideoBitrate * 2}k",
            "-g", "60",
            "-sc_threshold", "0"
        ];
    }

    private static string[] GetAudioCodecArgs(Stream stream)
    {
        if (stream.AudioCodec == "copy")
        {
            return ["-c:a", "copy"];
        }

        return
        [
            "-c:a", stream.AudioCodec,
            "-b:a", $"{stream.AudioBitrate}k",
            "-ar", "48000"
        ];
    }
}