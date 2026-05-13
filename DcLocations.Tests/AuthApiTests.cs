using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DcLocations.Tests;

public class AuthApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthApiTests(
        WebApplicationFactory<Program> factory
    )
    {
        _client =
            factory.CreateClient();
    }

    [Fact]
    public async Task Login_Endpoint_Exists()
    {
        var loginData =
            new
            {
                email = "test@test.com",
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
    public async Task Login_With_Missing_Password_Returns_BadRequest()
    {
        var loginData =
            new
            {
                email = "test@test.com",
                username = "test",
                password = ""
            };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/login",
                loginData
            );

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_With_Invalid_Email_Returns_BadRequest()
    {
        var registerData =
            new
            {
                username = "abc",
                email = "not-an-email",
                password = "password123"
            };

        var response =
            await _client.PostAsJsonAsync(
                "/api/auth/register",
                registerData
            );

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.BadRequest);
    }
}