using DcLocations.Api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

builder.Services.AddSingleton<DatabaseConnection>();

builder.Services.AddHttpClient(
    "PokemonApi",
    client =>
    {
        var baseUrl =
            builder.Configuration["PokemonApi:BaseUrl"]
            ?? "http://host.docker.internal:8080";

        client.BaseAddress =
            new Uri(baseUrl);
    }
);

var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? "THIS_IS_MY_SUPER_SECRET_DC_LOCATIONS_KEY_2026";

var key =
    Encoding.UTF8.GetBytes(jwtKey);

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,

                ValidateAudience = false,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(key)
            };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}