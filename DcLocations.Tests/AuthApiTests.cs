using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DcLocations.Tests
{
    public class AuthApiTests :
        IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthApiTests(
            WebApplicationFactory<Program> factory
        )
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // ARRANGE
            var loginRequest = new
            {
                email = "admin@dc.com",
                password = "password123"
            };

            // ACT
            var response =
                await _client.PostAsJsonAsync(
                    "/api/auth/login",
                    loginRequest
                );

            // ASSERT
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // ARRANGE
            var loginRequest = new
            {
                email = "fake@dc.com",
                password = "wrongpassword"
            };

            // ACT
            var response =
                await _client.PostAsJsonAsync(
                    "/api/auth/login",
                    loginRequest
                );

            // ASSERT
            response.StatusCode
                .Should()
                .Be(HttpStatusCode.Unauthorized);
        }
    }
}