using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DcLocations.Tests;

public class FavoritesApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public FavoritesApiTests(
        WebApplicationFactory<Program> factory
    )
    {
        _client =
            factory.CreateClient();
    }

    [Fact]
    public async Task Favorites_Without_Token_Returns_Unauthorized()
    {
        var response =
            await _client.GetAsync(
                "/api/favorites"
            );

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Add_Favorite_Without_Token_Returns_Unauthorized()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/favorites",
                new
                {
                    locationId = 1
                }
            );

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Remove_Favorite_Without_Token_Returns_Unauthorized()
    {
        var response =
            await _client.DeleteAsync(
                "/api/favorites/1"
            );

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }
}