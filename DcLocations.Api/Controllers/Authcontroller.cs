using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid registration data"
                });
            }

            try
            {
                using var connection =
                    _database.CreateConnection();

                await connection.OpenAsync();

                var checkQuery = @"
                    SELECT COUNT(*)
                    FROM users
                    WHERE username = @username
                    OR email = @email;
                ";

                using var checkCommand =
                    new MySqlCommand(checkQuery, connection);

                checkCommand.Parameters.AddWithValue(
                    "@username",
                    user.Username
                );

                checkCommand.Parameters.AddWithValue(
                    "@email",
                    user.Email
                );

                var existingCount =
                    Convert.ToInt32(
                        await checkCommand.ExecuteScalarAsync()
                    );

                if (existingCount > 0)
                {
                    return BadRequest(new
                    {
                        error = "Username or email already exists"
                    });
                }

                var insertQuery = @"
                    INSERT INTO users
                    (username, email, password_hash, role)
                    VALUES
                    (@username, @email, @passwordHash, 'User');
                ";

                using var insertCommand =
                    new MySqlCommand(insertQuery, connection);

                insertCommand.Parameters.AddWithValue(
                    "@username",
                    user.Username
                );

                insertCommand.Parameters.AddWithValue(
                    "@email",
                    user.Email
                );

                insertCommand.Parameters.AddWithValue(
                    "@passwordHash",
                    HashPassword(user.Password)
                );

                await insertCommand.ExecuteNonQueryAsync();

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
            if (string.IsNullOrWhiteSpace(user.Email)
                && string.IsNullOrWhiteSpace(user.Username))
            {
                return BadRequest(new
                {
                    error = "Email or username is required"
                });
            }

            if (string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest(new
                {
                    error = "Password is required"
                });
            }

            try
            {
                using var connection =
                    _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    SELECT
                        id,
                        username,
                        email,
                        password_hash,
                        role
                    FROM users
                    WHERE email = @login
                    OR username = @login
                    LIMIT 1;
                ";

                using var command =
                    new MySqlCommand(query, connection);

                var loginValue =
                    string.IsNullOrWhiteSpace(user.Email)
                        ? user.Username
                        : user.Email;

                command.Parameters.AddWithValue(
                    "@login",
                    loginValue
                );

                using var reader =
                    await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return Unauthorized(new
                    {
                        error = "Invalid credentials"
                    });
                }

                var passwordHash =
                    reader.GetString("password_hash");

                if (passwordHash != HashPassword(user.Password))
                {
                    return Unauthorized(new
                    {
                        error = "Invalid credentials"
                    });
                }

                var userId =
                    reader.GetInt32("id");

                var username =
                    reader.GetString("username");

                var email =
                    reader.GetString("email");

                var role =
                    reader.GetString("role");

                var token =
                    GenerateJwtToken(
                        userId,
                        username,
                        email,
                        role
                    );

                return Ok(new
                {
                    message = "Login successful",
                    userId = userId,
                    username = username,
                    email = email,
                    role = role,
                    token = token
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

        private string GenerateJwtToken(
            int userId,
            string username,
            string email,
            string role
        )
        {
            var claims =
                new[]
                {
                    new Claim(
                        ClaimTypes.NameIdentifier,
                        userId.ToString()
                    ),
                    new Claim(
                        ClaimTypes.Name,
                        username
                    ),
                    new Claim(
                        ClaimTypes.Email,
                        email
                    ),
                    new Claim(
                        ClaimTypes.Role,
                        role
                    )
                };

            var jwtKey =
                _configuration["Jwt:Key"]
                ?? "THIS_IS_MY_SUPER_SECRET_DC_LOCATIONS_KEY_2026";

            var key =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtKey)
                );

            var credentials =
                new SigningCredentials(
                    key,
                    SecurityAlgorithms.HmacSha256
                );

            var token =
                new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public static string HashPassword(string password)
        {
            var bytes =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(password)
                );

            return Convert.ToHexString(bytes);
        }
    }
}