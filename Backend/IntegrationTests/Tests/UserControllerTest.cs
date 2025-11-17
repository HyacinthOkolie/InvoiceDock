using System.Net;
using FluentAssertions;
using IntegrationTests.Infrastructure;

public class UserControllerTest : IntegrationTestBase
{
    public UserControllerTest(CustomWebApplicationFactory factory)
        : base(factory) { }

    [Fact]
    public async Task GetUsers_ReturnsSuccess_WhenCalled()
    {
        // Act - Use the correct endpoint
        var response = await Client.GetAsync("/WeatherForecast/users");

        // Assert - Just check it doesn't crash
        response.EnsureSuccessStatusCode(); // Throws if not 200-299
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Database_ShouldBeAccessible()
    {
        // Act
        var canConnect = await DbContext.Database.CanConnectAsync();

        // Assert
        Assert.True(canConnect);
    }

    [Fact]
    public async Task GetUsers_ReturnSeededUsers()
    {
        var response = await Client.GetAsync("/WeatherForecast/users");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await DeserializeResponseAsync<List<UserDto>>(response);
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }
}

public class UserDto
{
    public string FullName { get; set; }
    public string Email { get; set; }
}
