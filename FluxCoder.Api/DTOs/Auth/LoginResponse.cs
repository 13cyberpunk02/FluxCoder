namespace FluxCoder.Api.DTOs.Auth;

public record LoginResponse(string Token, string Username, string Role);