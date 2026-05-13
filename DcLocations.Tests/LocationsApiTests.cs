using DcLocations.Api.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DcLocations.Tests;

public class LocationsApiTests :
    IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public LocationsApiTests(
        WebApplicationFactory<Program> factory
    )
    {
        _client =
            factory.CreateClient();
    }

    [Fact]
    public async Task Locations_Endpoint_Exists()
    {
        var response =
            await _client.GetAsync(
                "/api/locations"
            );

        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_Location_Without_Token_Returns_Unauthorized()
    {
        var response =
            await _client.PostAsJsonAsync(
                "/api/locations",
                new Location
                {
                    Name = "Test Location",
                    Category = "City",
                    Description = "A test location"
                }
            );

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public void Location_Model_Requires_Name()
    {
        var location =
            new Location
            {
                Name = "",
                Category = "City",
                Description = "A test description"
            };

        var results =
            ValidateModel(location);

        results
            .Should()
            .Contain(result =>
                result.MemberNames.Contains("Name")
            );
    }

    [Fact]
    public void Location_Model_Requires_Category()
    {
        var location =
            new Location
            {
                Name = "Gotham",
                Category = "",
                Description = "A test description"
            };

        var results =
            ValidateModel(location);

        results
            .Should()
            .Contain(result =>
                result.MemberNames.Contains("Category")
            );
    }

    [Fact]
    public void Location_Model_Requires_Description()
    {
        var location =
            new Location
            {
                Name = "Gotham",
                Category = "City",
                Description = ""
            };

        var results =
            ValidateModel(location);

        results
            .Should()
            .Contain(result =>
                result.MemberNames.Contains("Description")
            );
    }

    private static List<ValidationResult> ValidateModel(
        object model
    )
    {
        var context =
            new ValidationContext(model);

        var results =
            new List<ValidationResult>();

        Validator.TryValidateObject(
            model,
            context,
            results,
            true
        );

        return results;
    }
}