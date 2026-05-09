using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace DcLocations.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseConnection _databaseConnection;

        public AuthController(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register(User user)
        {
            try
            {
                using MySqlConnection connection =
                    _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                // CHECK EMAIL
                string emailQuery =
                    "SELECT COUNT(*) FROM users WHERE email = @email";

                using MySqlCommand emailCommand =
                    new MySqlCommand(emailQuery, connection);

                emailCommand.Parameters.AddWithValue(
                    "@email",
                    user.Email
                );

                int existingEmails =
                    Convert.ToInt32(
                        await emailCommand.ExecuteScalarAsync()
                    );

                if (existingEmails > 0)
                {
                    return BadRequest(new
                    {
                        error = "Email already exists"
                    });
                }

                // CHECK USERNAME
                string usernameQuery =
                    "SELECT COUNT(*) FROM users WHERE username = @username";

                using MySqlCommand usernameCommand =
                    new MySqlCommand(usernameQuery, connection);

                usernameCommand.Parameters.AddWithValue(
                    "@username",
                    user.Username
                );

                int existingUsers =
                    Convert.ToInt32(
                        await usernameCommand.ExecuteScalarAsync()
                    );

                if (existingUsers > 0)
                {
                    return BadRequest(new
                    {
                        error = "Username already exists"
                    });
                }

                // INSERT USER
                string insertQuery = @"
                    INSERT INTO users
                    (username, email, password_hash)
                    VALUES
                    (@username, @email, @password_hash)
                ";

                using MySqlCommand insertCommand =
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
                    "@password_hash",
                    user.Password
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

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(User user)
        {
            try
            {
                using MySqlConnection connection =
                    _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                string query = @"
                    SELECT *
                    FROM users
                    WHERE email = @email
                    AND password_hash = @password_hash
                ";

                using MySqlCommand command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@email",
                    user.Email
                );

                command.Parameters.AddWithValue(
                    "@password_hash",
                    user.Password
                );

                using MySqlDataReader reader =
                    await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return Unauthorized(new
                    {
                        error = "Invalid email or password"
                    });
                }

                return Ok(new
                {
                    message = "Login successful"
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
    }
}