using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace DcLocations.Api.Controllers
{
    [ApiController]
    [Route("api/favorites")]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly DatabaseConnection _database;

        public FavoritesController(DatabaseConnection database)
        {
            _database = database;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFavorites(int userId)
        {
            try
            {
                using var connection =
                    _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    SELECT l.*
                    FROM favorites f
                    JOIN locations l
                        ON f.location_id = l.id
                    WHERE f.user_id = @userId
                ";

                using var command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@userId",
                    userId
                );

                using var reader =
                    await command.ExecuteReaderAsync();

                var favorites =
                    new List<Location>();

                while (await reader.ReadAsync())
                {
                    favorites.Add(new Location
                    {
                        Id =
                            reader.GetInt32("id"),

                        Name =
                            reader.GetString("name"),

                        Category =
                            reader.GetString("category"),

                        Description =
                            reader.GetString("description"),

                        AssociatedHero =
                            reader.GetString("associated_hero"),

                        UniverseRegion =
                            reader.GetString("universe_region"),

                        FirstAppearance =
                            reader.GetString("first_appearance"),

                        ImageUrl =
                            reader.GetString("image_url")
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

        [HttpPost]
        public async Task<IActionResult> AddFavorite(
            [FromBody] Favorite favorite
        )
        {
            try
            {
                using var connection =
                    _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    INSERT INTO favorites
                    (user_id, location_id)
                    VALUES
                    (@userId, @locationId)
                ";

                using var command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@userId",
                    favorite.UserId
                );

                command.Parameters.AddWithValue(
                    "@locationId",
                    favorite.LocationId
                );

                await command.ExecuteNonQueryAsync();

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

        [HttpDelete]
        public async Task<IActionResult> RemoveFavorite(
            int userId,
            int locationId
        )
        {
            try
            {
                using var connection =
                    _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    DELETE FROM favorites
                    WHERE user_id = @userId
                    AND location_id = @locationId
                ";

                using var command =
                    new MySqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@userId",
                    userId
                );

                command.Parameters.AddWithValue(
                    "@locationId",
                    locationId
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

    public class Favorite
    {
        public int UserId { get; set; }

        public int LocationId { get; set; }
    }
}