using FluentAssertions;
using SoftOne.Api.Tests.TestHelpers;

namespace SoftOne.Api.Tests.Endpoints;

public class HealthEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.IsSuccessStatusCode.Should().BeTrue();
    }
}
