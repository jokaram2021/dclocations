using DcLocations.Api.Data;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.IdentityModel.Tokens;

using Microsoft.OpenApi.Models;

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



var jwtKey =
    builder.Configuration["Jwt:Key"]
    ?? "SuperSecretDevelopmentKey12345";

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



var app = builder.Build();



app.UseSwagger();



app.UseSwaggerUI();



app.UseCors("AllowAll");



app.UseStaticFiles();



app.UseAuthentication();



app.UseAuthorization();



app.MapControllers();



app.Run();



public partial class Program
{
}