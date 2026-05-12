using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DcLocations.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseConnection _database;

        private readonly IConfiguration _configuration;

        public AuthController(
            DatabaseConnection database,
            IConfiguration configuration
        )
        {
            _database = database;

            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                using var connection =
                    _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    INSERT INTO users
                    (username, email, password_hash, role)
                    VALUES
                    (@username, @email, @password, 'User')
                ";

                using var command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@username",
                    user.Username
                );

                command.Parameters.AddWithValue(
                    "@email",
                    user.Email
                );

                command.Parameters.AddWithValue(
                    "@password",
                    user.Password
                );

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Registration failed",
                    details = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(User user)
        {
            try
            {
                using var connection =
                    _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    SELECT *
                    FROM users
                    WHERE email = @email
                    AND password_hash = @password
                ";

                using var command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@email",
                    user.Email
                );

                command.Parameters.AddWithValue(
                    "@password",
                    user.Password
                );

                using var reader =
                    await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var token =
                        GenerateJwtToken(user.Email!);

                    return Ok(new
                    {
                        message = "Login successful",
                        token = token,
                        email = user.Email
                    });
                }

                return Unauthorized(new
                {
                    error = "Invalid credentials"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Login failed",
                    details = ex.Message
                });
            }
        }

        private string GenerateJwtToken(string email)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Email, email)
            };

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        _configuration["Jwt:Key"]!
                    )
                );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }
    }
}