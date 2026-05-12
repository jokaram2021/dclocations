using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DcLocations.Tests
{
    public class LocationsApiTests :
        IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public LocationsApiTests(
            WebApplicationFactory<Program> factory
        )
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetLocations_ReturnsSuccessStatus()
        {
            var response =
                await _client.GetAsync("/api/locations");

            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetLocations_ReturnsLocationData()
        {
            var locations =
                await _client.GetFromJsonAsync<List<object>>(
                    "/api/locations"
                );

            locations.Should().NotBeNull();

            locations.Count.Should().BeGreaterThan(0);
        }
    }
}