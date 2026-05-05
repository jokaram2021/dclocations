using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector.MySql;

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
            List<Location> locations = new List<Location>();

            try
            {
                using MySqlConnection connection = _databaseConnection.CreateConnection();
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        id,
                        name,
                        category,
                        description,
                        associated_hero,
                        universe_region,
                        first_appearance,
                        image_url
                    FROM locations;
                ";

                using MySqlCommand command = new MySqlCommand(query, connection);
                using MySqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    Location location = new Location
                    {
                        Id = reader.GetInt32("id"),
                        Name = reader.GetString("name"),
                        Category = reader.GetString("category"),
                        Description = reader.GetString("description"),
                        AssociatedHero = reader.IsDBNull(reader.GetOrdinal("associated_hero"))
                            ? null
                            : reader.GetString("associated_hero"),
                        UniverseRegion = reader.IsDBNull(reader.GetOrdinal("universe_region"))
                            ? null
                            : reader.GetString("universe_region"),
                        FirstAppearance = reader.IsDBNull(reader.GetOrdinal("first_appearance"))
                            ? null
                            : reader.GetString("first_appearance"),
                        ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url"))
                            ? null
                            : reader.GetString("image_url")
                    };

                    locations.Add(location);
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
                using MySqlConnection connection = _databaseConnection.CreateConnection();
                await connection.OpenAsync();

                string query = @"
                    SELECT 
                        id,
                        name,
                        category,
                        description,
                        associated_hero,
                        universe_region,
                        first_appearance,
                        image_url
                    FROM locations
                    WHERE id = @id;
                ";

                using MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);

                using MySqlDataReader reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    return NotFound(new
                    {
                        error = "Location not found"
                    });
                }

                Location location = new Location
                {
                    Id = reader.GetInt32("id"),
                    Name = reader.GetString("name"),
                    Category = reader.GetString("category"),
                    Description = reader.GetString("description"),
                    AssociatedHero = reader.IsDBNull(reader.GetOrdinal("associated_hero"))
                        ? null
                        : reader.GetString("associated_hero"),
                    UniverseRegion = reader.IsDBNull(reader.GetOrdinal("universe_region"))
                        ? null
                        : reader.GetString("universe_region"),
                    FirstAppearance = reader.IsDBNull(reader.GetOrdinal("first_appearance"))
                        ? null
                        : reader.GetString("first_appearance"),
                    ImageUrl = reader.IsDBNull(reader.GetOrdinal("image_url"))
                        ? null
                        : reader.GetString("image_url")
                };

                return Ok(location);
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
    }
}