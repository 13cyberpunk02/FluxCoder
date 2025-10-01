using FluxCoder.Api.Data;
using FluxCoder.Api.DTOs.Auth;
using FluxCoder.Api.Models;
using FluxCoder.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FluxCoder.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        group.MapPost("login", LoginAsync)
            .WithName("Login")
            .WithSummary("Авторизация пользователя по имени пользователя и паролю")
            .Produces<string>(403)
            .Produces<LoginResponse>();

        group.MapPost("register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Регистрация нового пользователя")
            .Produces<string>()
            .Produces<string>(400)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithTags("Authentication");;
            
        
        return group;
    }
    
    private static async Task<IResult> LoginAsync(
        [FromBody]LoginRequest request,     
        AppDbContext db,
        JwtService  jwtService,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.UserName, cancellationToken);
        
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Results.Unauthorized();
        }

        if (!user.IsActive)
        {
            return Results.Problem("Пользователь не активен", statusCode: StatusCodes.Status403Forbidden);            
        }

        var token = jwtService.GenerateJwtToken(user.Username, user.Role, user.Id);
        
        return Results.Ok(new LoginResponse(token, user.Username, user.Role));
    }
    
    private static async Task<IResult> RegisterAsync(
        [FromBody]RegisterRequest request,
        AppDbContext db,
        JwtService  jwtService,
        CancellationToken cancellationToken = default)
    {
        if (await db.Users.AnyAsync(u => u.Username == request.Username, cancellationToken: cancellationToken))
        {
            return Results.BadRequest("Пользователь уже зарегистрирован");
        }
        
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        
        var user = new User
        {
            Username = request.Username,
            PasswordHash = passwordHash,
            Role = request.Role
        };
        
        await db.Users.AddAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { message = "Успешная регистрация", userId = user.Id });
    }
}