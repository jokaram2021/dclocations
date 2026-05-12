using Xunit;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

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
    public async Task Login_Endpoint_Responds()
    {
        var loginData = new
        {
            username = "admin",
            password = "password123"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginData
            );

        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var loginData = new
        {
            username = "fakeuser",
            password = "wrongpassword"
        };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginData
            );

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}