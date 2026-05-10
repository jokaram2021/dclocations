using DcLocations.Api.Data;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace DcLocations.Api.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    public class FavoritesController : ControllerBase
    {
        private readonly DatabaseConnection _databaseConnection;

        public FavoritesController(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        // ADD FAVORITE
        [HttpPost]
        public async Task<IActionResult> AddFavorite(
            [FromBody] FavoriteRequest request
        )
        {
            try
            {
                using MySqlConnection connection =
                    _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                // CHECK DUPLICATE
                string checkQuery = @"
                    SELECT COUNT(*)
                    FROM favorites
                    WHERE user_id = @user_id
                    AND location_id = @location_id
                ";

                using MySqlCommand checkCommand =
                    new MySqlCommand(checkQuery, connection);

                checkCommand.Parameters.AddWithValue(
                    "@user_id",
                    request.UserId
                );

                checkCommand.Parameters.AddWithValue(
                    "@location_id",
                    request.LocationId
                );

                int existing =
                    Convert.ToInt32(
                        await checkCommand.ExecuteScalarAsync()
                    );

                if (existing > 0)
                {
                    return BadRequest(new
                    {
                        error = "Favorite already exists"
                    });
                }

                // INSERT FAVORITE
                string insertQuery = @"
                    INSERT INTO favorites
                    (user_id, location_id)
                    VALUES
                    (@user_id, @location_id)
                ";

                using MySqlCommand insertCommand =
                    new MySqlCommand(insertQuery, connection);

                insertCommand.Parameters.AddWithValue(
                    "@user_id",
                    request.UserId
                );

                insertCommand.Parameters.AddWithValue(
                    "@location_id",
                    request.LocationId
                );

                await insertCommand.ExecuteNonQueryAsync();

                return Ok(new
                {
                    message = "Favorite added"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to add favorite",
                    details = ex.Message
                });
            }
        }

        // GET USER FAVORITES
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            try
            {
                List<object> favorites = new();

                using MySqlConnection connection =
                    _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                string query = @"
                    SELECT
                        l.id,
                        l.name,
                        l.category,
                        l.description
                    FROM favorites f
                    JOIN locations l
                        ON f.location_id = l.id
                    WHERE f.user_id = @user_id
                ";

                using MySqlCommand command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@user_id",
                    userId
                );

                using MySqlDataReader reader =
                    await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    favorites.Add(new
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Category = reader.GetString("category"),
                        Description = reader.GetString("description")
                    });
                }

                return Ok(favorites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to load favorites",
                    details = ex.Message
                });
            }
        }

        // DELETE FAVORITE
        [HttpDelete]
        public async Task<IActionResult> DeleteFavorite(
            [FromBody] FavoriteRequest request
        )
        {
            try
            {
                using MySqlConnection connection =
                    _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                string query = @"
                    DELETE FROM favorites
                    WHERE user_id = @user_id
                    AND location_id = @location_id
                ";

                using MySqlCommand command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@user_id",
                    request.UserId
                );

                command.Parameters.AddWithValue(
                    "@location_id",
                    request.LocationId
                );

                await command.ExecuteNonQueryAsync();

                return Ok(new
                {
                    message = "Favorite removed"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to remove favorite",
                    details = ex.Message
                });
            }
        }
    }

    // REQUEST MODEL
    public class FavoriteRequest
    {
        public int UserId { get; set; }

        public int LocationId { get; set; }
    }
}