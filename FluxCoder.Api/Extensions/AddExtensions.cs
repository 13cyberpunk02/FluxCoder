using System.Text;
using FluxCoder.Api.Data;
using FluxCoder.Api.Models.Settings;
using FluxCoder.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FluxCoder.Api.Extensions;

public static class AddExtensions
{
    
    public static IServiceCollection AddAllExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Postgres:ConnectionString"] 
                               ?? throw new NullReferenceException("В переменных окружения не добавлен путь к БД Postgres");
        
        services.AddDatabase(connectionString);
        services.AddOwnAuthentication(configuration);
        services.AddCustomCors();
        services.AddScoped<JwtService>();
        services.AddScoped<StreamService>();
        return services;
    } 
    
    private static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });
        return services;
    }

    private static IServiceCollection AddOwnAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();
        if (jwtSettings == null)
            throw new NullReferenceException("В переменных окружения не настроен JWT");

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });
        services.AddAuthorization();
        return services;
    }

    private static IServiceCollection AddCustomCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAngular", policy =>
            {
                policy.WithOrigins("http://localhost:4200")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
        return services;
    }
    
}