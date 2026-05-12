using Xunit;
using System.Net;
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
    public async Task GetLocations_ReturnsSuccessStatus()
    {
        var response =
            await _client.GetAsync("/api/locations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLocations_ReturnsLocationData()
    {
        var response =
            await _client.GetAsync("/api/locations");

        response.EnsureSuccessStatusCode();

        var content =
            await response.Content.ReadAsStringAsync();

        content.Should().Contain("Gotham");
    }
}