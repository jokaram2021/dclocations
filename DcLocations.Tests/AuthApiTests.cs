using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

namespace DcLocations.Tests;

public class AuthApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_Endpoint_Exists()
    {
        var loginData = new
        {
            username = "test",
            password = "test"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginData
            );

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_Endpoint_Responds()
    {
        var loginData = new
        {
            username = "test",
            password = "test"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginData
            );

        ((int)response.StatusCode)
            .Should()
            .BeGreaterThan(0);
    }
}