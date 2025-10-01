using FluxCoder.Api.Extensions;
using FluxCoder.Api.Models.Settings;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("./Environments/AppEnvironments.json", optional: true, reloadOnChange: true);
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));


builder.Services.AddOpenApi();
builder.Services.AddOpenApiExtension();
builder.Services.AddAllExtensions(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Flux Coder API");
        options.WithTheme(ScalarTheme.BluePlanet);
    });
}
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
app.MapAllEndpoints();
app.Run();
