using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Security.Claims;

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

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    error = "Invalid user token"
                });
            }

            try
            {
                using var connection = _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    SELECT
                        l.id,
                        l.name,
                        l.category,
                        l.description,
                        l.associated_hero,
                        l.universe_region,
                        l.first_appearance,
                        l.image_url,
                        l.wiki_url
                    FROM favorites f
                    JOIN locations l
                        ON f.location_id = l.id
                    WHERE f.user_id = @userId
                    ORDER BY l.name;
                ";

                using var command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@userId", userId.Value);

                using var reader = await command.ExecuteReaderAsync();

                var favorites = new List<Location>();

                while (await reader.ReadAsync())
                {
                    favorites.Add(MapLocation(reader));
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
        public async Task<IActionResult> AddFavorite([FromBody] FavoriteRequest favorite)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    error = "Invalid user token"
                });
            }

            if (favorite.LocationId <= 0)
            {
                return BadRequest(new
                {
                    error = "A valid locationId is required"
                });
            }

            try
            {
                using var connection = _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    INSERT IGNORE INTO favorites
                    (user_id, location_id)
                    VALUES
                    (@userId, @locationId);
                ";

                using var command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@userId", userId.Value);
                command.Parameters.AddWithValue("@locationId", favorite.LocationId);

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

        [HttpDelete("{locationId}")]
        public async Task<IActionResult> RemoveFavorite(int locationId)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized(new
                {
                    error = "Invalid user token"
                });
            }

            try
            {
                using var connection = _database.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    DELETE FROM favorites
                    WHERE user_id = @userId
                    AND location_id = @locationId;
                ";

                using var command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@userId", userId.Value);
                command.Parameters.AddWithValue("@locationId", locationId);

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

        private int? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(userIdValue, out var userId))
            {
                return userId;
            }

            return null;
        }

        private static Location MapLocation(MySqlDataReader reader)
        {
            return new Location
            {
                Id = reader.GetInt32("id"),

                Name = reader.GetString("name"),

                Category = reader.GetString("category"),

                Description = reader.GetString("description"),

                AssociatedHero =
                    reader.IsDBNull(reader.GetOrdinal("associated_hero"))
                        ? null
                        : reader.GetString("associated_hero"),

                UniverseRegion =
                    reader.IsDBNull(reader.GetOrdinal("universe_region"))
                        ? null
                        : reader.GetString("universe_region"),

                FirstAppearance =
                    reader.IsDBNull(reader.GetOrdinal("first_appearance"))
                        ? null
                        : reader.GetString("first_appearance"),

                ImageUrl =
                    reader.IsDBNull(reader.GetOrdinal("image_url"))
                        ? null
                        : reader.GetString("image_url"),

                WikiUrl =
                    reader.IsDBNull(reader.GetOrdinal("wiki_url"))
                        ? null
                        : reader.GetString("wiki_url")
            };
        }
    }

    public class FavoriteRequest
    {
        public int LocationId { get; set; }
    }
}