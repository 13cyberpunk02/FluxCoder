using System.Security.Claims;

namespace FluxCoder.Api.Services;

public class UserContextService
{
    public static int GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            throw new UnauthorizedAccessException("ID пользователя не найдено в токене");

        return userId;
    }

    public static string GetUserRole(ClaimsPrincipal user)
    {
        var roleClaim = user.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value ?? "User";
    }

    public static bool IsAdmin(ClaimsPrincipal user)
    {
        return GetUserRole(user) == "Admin";
    }
}