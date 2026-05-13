using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace DcLocations.Api.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : ControllerBase
    {
        private readonly DatabaseConnection _databaseConnection;

        public LocationsController(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllLocations()
        {
            var locations = new List<Location>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    SELECT
                        id,
                        name,
                        category,
                        description,
                        associated_hero,
                        universe_region,
                        first_appearance,
                        image_url,
                        wiki_url
                    FROM locations
                    ORDER BY name;
                ";

                using var command = new MySqlCommand(query, connection);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    locations.Add(MapLocation(reader));
                }

                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to retrieve locations",
                    details = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLocationById(int id)
        {
            try
            {
                using var connection = _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    SELECT
                        id,
                        name,
                        category,
                        description,
                        associated_hero,
                        universe_region,
                        first_appearance,
                        image_url,
                        wiki_url
                    FROM locations
                    WHERE id = @id;
                ";

                using var command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        error = "Location not found"
                    });
                }

                return Ok(MapLocation(reader));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to retrieve location",
                    details = ex.Message
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateLocation(Location location)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid location data"
                });
            }

            try
            {
                using var connection = _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    INSERT INTO locations
                    (
                        name,
                        category,
                        description,
                        associated_hero,
                        universe_region,
                        first_appearance,
                        image_url,
                        wiki_url
                    )
                    VALUES
                    (
                        @name,
                        @category,
                        @description,
                        @associatedHero,
                        @universeRegion,
                        @firstAppearance,
                        @imageUrl,
                        @wikiUrl
                    );

                    SELECT LAST_INSERT_ID();
                ";

                using var command = new MySqlCommand(query, connection);

                AddLocationParameters(command, location);

                var newId = Convert.ToInt32(await command.ExecuteScalarAsync());

                location.Id = newId;

                return CreatedAtAction(
                    nameof(GetLocationById),
                    new { id = newId },
                    location
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to create location",
                    details = ex.Message
                });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateLocation(int id, Location location)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    error = "Invalid location data"
                });
            }

            try
            {
                using var connection = _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    UPDATE locations
                    SET
                        name = @name,
                        category = @category,
                        description = @description,
                        associated_hero = @associatedHero,
                        universe_region = @universeRegion,
                        first_appearance = @firstAppearance,
                        image_url = @imageUrl,
                        wiki_url = @wikiUrl
                    WHERE id = @id;
                ";

                using var command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", id);

                AddLocationParameters(command, location);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound(new
                    {
                        error = "Location not found"
                    });
                }

                location.Id = id;

                return Ok(location);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to update location",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            try
            {
                using var connection = _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                var query = @"
                    DELETE FROM locations
                    WHERE id = @id;
                ";

                using var command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", id);

                var rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound(new
                    {
                        error = "Location not found"
                    });
                }

                return Ok(new
                {
                    message = "Location deleted"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to delete location",
                    details = ex.Message
                });
            }
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

        private static void AddLocationParameters(MySqlCommand command, Location location)
        {
            command.Parameters.AddWithValue("@name", location.Name);

            command.Parameters.AddWithValue("@category", location.Category);

            command.Parameters.AddWithValue("@description", location.Description);

            command.Parameters.AddWithValue(
                "@associatedHero",
                location.AssociatedHero ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@universeRegion",
                location.UniverseRegion ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@firstAppearance",
                location.FirstAppearance ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@imageUrl",
                location.ImageUrl ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@wikiUrl",
                location.WikiUrl ?? (object)DBNull.Value
            );
        }
    }
}