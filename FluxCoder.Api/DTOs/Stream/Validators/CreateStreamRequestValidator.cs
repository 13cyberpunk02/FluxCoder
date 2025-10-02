using FluentValidation;

namespace FluxCoder.Api.DTOs.Stream.Validators;

public class CreateStreamRequestValidator : AbstractValidator<CreateStreamRequest>
{
    private static readonly string[] ValidVideoCodecs = { "libx264", "libx265", "copy" };
    private static readonly string[] ValidAudioCodecs = { "aac", "mp3", "copy" };
    private static readonly string[] ValidQualities = { "low", "medium", "high" };

    public CreateStreamRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Наименование видео потока обязателен к заполнению")
            .MaximumLength(100).WithMessage("Наименование не должно превышать 100 символов.");

        RuleFor(x => x.InputUrl)
            .NotEmpty().WithMessage("Укажите URL-адрес")
            .Must(BeValidUrl).WithMessage("Входной URL-адрес должен быть формата: (http/https/rtmp/rtsp)");

        RuleFor(x => x.StreamKey)
            .NotEmpty().WithMessage("Ключ потока обязателен к заполнению")
            .Matches("^[a-z0-9_-]+$").WithMessage("Ключ потока может содержать только строчные буквы, цифры, дефисы и символы подчеркивания.")
            .MaximumLength(50).WithMessage("Ключ потока не должен превышать 50 символов.");

        RuleFor(x => x.VideoCodec)
            .Must(codec => ValidVideoCodecs.Contains(codec))
            .WithMessage($"Видеокодек должен быть одним из: {string.Join(", ", ValidVideoCodecs)}");

        RuleFor(x => x.AudioCodec)
            .Must(codec => ValidAudioCodecs.Contains(codec))
            .WithMessage($"Аудиокодек должен быть одним из: {string.Join(", ", ValidAudioCodecs)}");

        RuleFor(x => x.Quality)
            .Must(quality => ValidQualities.Contains(quality))
            .WithMessage($"Качество должно быть одним из: {string.Join(", ", ValidQualities)}");

        RuleFor(x => x.VideoBitrate)
            .InclusiveBetween(500, 10000).WithMessage("Битрейт видео должен быть от 500 до 10000 кбит/с.");

        RuleFor(x => x.AudioBitrate)
            .InclusiveBetween(64, 320).WithMessage("Битрейт аудио должен быть от 64 до 320 кбит/с.");
    }

    private static bool BeValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return url.StartsWith("http://") || 
               url.StartsWith("https://") || 
               url.StartsWith("rtmp://") || 
               url.StartsWith("rtsp://");
    }
}