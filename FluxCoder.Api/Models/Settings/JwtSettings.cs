namespace FluxCoder.Api.Models.Settings;

public class JwtSettings
{
    public string Key { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
}