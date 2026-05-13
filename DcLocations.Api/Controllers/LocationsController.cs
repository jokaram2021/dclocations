using DcLocations.Api.Data;
using DcLocations.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DcLocations.Api.Controllers
{
    [ApiController]
    [Route("api/locations")]
    public class LocationsController : ControllerBase
    {
        private readonly DatabaseConnection _databaseConnection;

        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IConfiguration _configuration;

        public LocationsController(
            DatabaseConnection databaseConnection,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration
        )
        {
            _databaseConnection = databaseConnection;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
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
                        wiki_url,
                        pokemon_name,
                        view_count
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

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeaturedLocations()
        {
            var locations = new List<Location>();

            try
            {
                using var connection = _databaseConnection.CreateConnection();

                await connection.OpenAsync();

                var popularQuery = @"
                    SELECT
                        id,
                        name,
                        category,
                        description,
                        associated_hero,
                        universe_region,
                        first_appearance,
                        image_url,
                        wiki_url,
                        pokemon_name,
                        view_count
                    FROM locations
                    WHERE view_count > 0
                    ORDER BY view_count DESC, name ASC
                    LIMIT 3;
                ";

                using var popularCommand = new MySqlCommand(popularQuery, connection);

                using var popularReader = await popularCommand.ExecuteReaderAsync();

                while (await popularReader.ReadAsync())
                {
                    locations.Add(MapLocation(popularReader));
                }

                await popularReader.CloseAsync();

                if (locations.Count > 0)
                {
                    return Ok(locations);
                }

                var randomQuery = @"
                    SELECT
                        id,
                        name,
                        category,
                        description,
                        associated_hero,
                        universe_region,
                        first_appearance,
                        image_url,
                        wiki_url,
                        pokemon_name,
                        view_count
                    FROM locations
                    ORDER BY RAND()
                    LIMIT 3;
                ";

                using var randomCommand = new MySqlCommand(randomQuery, connection);

                using var randomReader = await randomCommand.ExecuteReaderAsync();

                while (await randomReader.ReadAsync())
                {
                    locations.Add(MapLocation(randomReader));
                }

                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to retrieve featured locations",
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

                var updateViewsQuery = @"
                    UPDATE locations
                    SET view_count = view_count + 1
                    WHERE id = @id;
                ";

                using var updateCommand = new MySqlCommand(updateViewsQuery, connection);

                updateCommand.Parameters.AddWithValue("@id", id);

                await updateCommand.ExecuteNonQueryAsync();

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
                        wiki_url,
                        pokemon_name,
                        view_count
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

        [HttpGet("{id}/pokemon-match")]
        public async Task<IActionResult> GetPokemonMatchForLocation(int id)
        {
            try
            {
                Location? location = null;

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
                        wiki_url,
                        pokemon_name,
                        view_count
                    FROM locations
                    WHERE id = @id;
                ";

                using var command = new MySqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", id);

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    location = MapLocation(reader);
                }

                if (location == null)
                {
                    return NotFound(new
                    {
                        error = "Location not found"
                    });
                }

                var pokemonName =
                    string.IsNullOrWhiteSpace(location.PokemonName)
                        ? "pikachu"
                        : location.PokemonName;

                var pokemonClient =
                    _httpClientFactory.CreateClient("PokemonApi");

                var token =
                    _configuration["PokemonApi:Token"]
                    ?? "Password1";

                pokemonClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token
                    );

                var pokemonResponse =
                    await pokemonClient.GetAsync(
                        $"/pokemon/{pokemonName}"
                    );

                if (!pokemonResponse.IsSuccessStatusCode)
                {
                    return StatusCode(502, new
                    {
                        error = "The match data could not be reached or returned an error.",
                        pokemonApiStatus = (int)pokemonResponse.StatusCode
                    });
                }

                var json =
                    await pokemonResponse.Content.ReadAsStringAsync();

                var pokemon =
                    JsonSerializer.Deserialize<PokemonApiResponse>(
                        json,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );

                if (pokemon == null)
                {
                    return StatusCode(502, new
                    {
                        error = "The match data returned invalid data."
                    });
                }

                return Ok(new
                {
                    locationId = location.Id,
                    locationName = location.Name,
                    pokemonId = pokemon.Id,
                    pokemonName = pokemon.Name,
                    types = pokemon.Types
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Failed to load energy match",
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
                        wiki_url,
                        pokemon_name,
                        view_count
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
                        @wikiUrl,
                        @pokemonName,
                        @viewCount
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
                        wiki_url = @wikiUrl,
                        pokemon_name = @pokemonName,
                        view_count = @viewCount
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
                        : reader.GetString("wiki_url"),

                PokemonName =
                    reader.IsDBNull(reader.GetOrdinal("pokemon_name"))
                        ? null
                        : reader.GetString("pokemon_name"),

                ViewCount =
                    reader.IsDBNull(reader.GetOrdinal("view_count"))
                        ? 0
                        : reader.GetInt32("view_count")
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

            command.Parameters.AddWithValue(
                "@pokemonName",
                location.PokemonName ?? (object)DBNull.Value
            );

            command.Parameters.AddWithValue(
                "@viewCount",
                location.ViewCount
            );
        }
    }

    public class PokemonApiResponse
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public List<string> Types { get; set; } = new List<string>();
    }
}