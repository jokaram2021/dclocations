using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DcLocations.Tests;

public class LocationsApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LocationsApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Locations_Endpoint_Exists()
    {
        var response =
            await _client.GetAsync("/api/locations");

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Locations_Endpoint_Responds()
    {
        var response =
            await _client.GetAsync("/api/locations");

        ((int)response.StatusCode)
            .Should()
            .BeGreaterThan(0);
    }
}