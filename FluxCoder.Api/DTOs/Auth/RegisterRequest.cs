namespace FluxCoder.Api.DTOs.Auth;

public record RegisterRequest(string Username, string Password, string ConfirmPassword, string Role = "User");