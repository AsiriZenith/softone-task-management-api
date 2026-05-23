using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SoftOne.Api.DTOs.Responses;
using SoftOne.Api.Tests.TestHelpers;

namespace SoftOne.Api.Tests.Endpoints;

public class TaskEndpointsTests : IClassFixture<SoftOneWebApplicationFactory>
{
    private const string ValidUsername = "admin";
    private const string ValidPassword = "Admin@123";
    private readonly HttpClient _client;

    public TaskEndpointsTests(SoftOneWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTasks_WithoutAuthorization_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTasks_WithValidBasicAuth_ReturnsOk()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/tasks");
        request.Headers.Authorization = CreateBasicAuthHeader();

        var response = await _client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiSuccessResponse<IReadOnlyList<TaskResponse>>>();
        body.Should().NotBeNull();
        body!.Success.Should().BeTrue();
        body.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            username = ValidUsername,
            password = ValidPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.Success.Should().BeTrue();
    }

    private static AuthenticationHeaderValue CreateBasicAuthHeader()
    {
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{ValidUsername}:{ValidPassword}"));

        return new AuthenticationHeaderValue("Basic", credentials);
    }
}
