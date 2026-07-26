using System.Net;
using System.Net.Http.Json;
using SecureEmployeeManagement.DTOs;
using Xunit;

namespace Tests;

public class EmployeesEndpointTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EmployeesEndpointTests(CustomWebApplicationFactory factory)
        => _factory = factory;

    [Fact]
    public async Task GetEmployees_NoToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_ThenAccessWithToken_Returns200()
    {
        var client = _factory.CreateClient();

        var reg = await client.PostAsJsonAsync("/api/auth/register",
            new { username = "itUser", email = "it@test.com", password = "password123" });
        var body = await reg.Content.ReadFromJsonAsync<AuthResponseDto>();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", body!.Token);

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_AsEmployeeRole_Returns403()
    {
        var client = _factory.CreateClient();

        var reg = await client.PostAsJsonAsync("/api/auth/register",
            new { username = "empUser", email = "emp@test.com", password = "password123" });
        var body = await reg.Content.ReadFromJsonAsync<AuthResponseDto>();

        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", body!.Token);

        var response = await client.PostAsJsonAsync("/api/employees",
            new { fullName = "X", email = "x@test.com", department = "IT", jobTitle = "Dev" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Register_InvalidInput_Returns400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { username = "", email = "notanemail", password = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}