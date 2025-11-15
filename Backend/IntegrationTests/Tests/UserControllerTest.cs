using System.Net;
using System.Net.Http.Json;
using API.Models;
using FluentAssertions;
using IntegrationTests.Infrastructure;

public class UserControllerTest : IntegrationTestBase
{
    public UserControllerTest(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetUsers_ReturnSeededUsers()
    {
        var response = await Client.GetAsync("/api/WeatherForecast/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await DeserializeResponse<List<User>>(response);

        // var users = await response.Content.ReadFromJsonAsync<List<User>>();

        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }
}
