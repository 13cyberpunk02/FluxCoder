using FluxCoder.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace FluxCoder.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("api/users")
            .WithTags("Управление пользователями")
            .RequireAuthorization()
            .WithOpenApi();

        group.MapGet("/", GetUsers)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Список всех пользователей")
            .WithName("GetUsers")
            .WithTags("Users");
        
        group.MapPut("/{id:int}/role", UpdateUserRole)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Редактирование роли пользователя")
            .WithName("UpdateUserRole")
            .WithTags("Users");
        
        group.MapPut("/{id:int}/status", UpdateUserStatus)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Редактирование статуса пользователя")
            .WithName("UpdateUserStatus")
            .WithTags("Users");
        
        group.MapDelete("/{id:int}", DeleteUser)
            .RequireAuthorization(policy => policy.RequireRole("Admin"))
            .WithSummary("Удаление пользователя")
            .WithName("DeleteUser")
            .WithTags("Users");
        
        return group;
    }

    private static async Task<IResult> GetUsers(AppDbContext db, CancellationToken ct = default)
    {
        var users = await db.Users.AsNoTracking()
            .Select(user => new
            {
                user.Id,
                user.Username,
                user.Role,
                user.IsActive,
                user.CreatedAt
            })
            .ToListAsync(ct);
        return Results.Ok(users);
    }

    private static async Task<IResult> UpdateUserRole(int id, string role, AppDbContext db,
        CancellationToken ct = default)
    {
        if (role != "User" && role != "Admin")
            return Results.BadRequest(new { message = "Роль должен быть либо 'User' либо 'Admin'" });

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound(new { message = "Пользователь не найден" });

        user.Role = role;
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { message = $"Роль пользователя заменен на: {role}" });
    }

    private static async Task<IResult> UpdateUserStatus(int id, bool isActive, AppDbContext db,
        CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound(new { message = "Пользователь не найден" });
        
        user.IsActive = isActive;
        await db.SaveChangesAsync(ct);
        
        return Results.Ok(new { message = $"Пользователь {(isActive ? "активирован" : "деактивирован")}" });
    }

    private static async Task<IResult> DeleteUser(int id, AppDbContext db, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null)
            return Results.NotFound(new { message = "Пользователь не найден" });
        
        if (user.Id == 1) // Защита админа по умолчанию
            return Results.BadRequest(new { message = "Запрещено системой удалять администратора" });

        db.Users.Remove(user);
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { message = "Пользователь удален успешно" });
    }
}