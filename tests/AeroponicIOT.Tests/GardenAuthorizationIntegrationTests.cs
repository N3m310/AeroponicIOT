using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AeroponicIOT.Data;
using AeroponicIOT.Models;
using AeroponicIOT.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AeroponicIOT.Tests;

public class GardenAuthorizationIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public GardenAuthorizationIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetGardenDevicesScopesResultsForFarmer()
    {
        await ResetDatabaseAsync(db =>
        {
            var farmer1 = new User { Id = 1, Username = "farmer-1", Email = "farmer1@test.local", PasswordHash = "hash", Role = "Farmer", CreatedAt = DateTime.UtcNow };
            var farmer2 = new User { Id = 2, Username = "farmer-2", Email = "farmer2@test.local", PasswordHash = "hash", Role = "Farmer", CreatedAt = DateTime.UtcNow };
            var admin = new User { Id = 99, Username = "admin", Email = "admin@test.local", PasswordHash = "hash", Role = "Administrator", CreatedAt = DateTime.UtcNow };

            db.Users.AddRange(farmer2, admin); // farmer1 is added implicitly via garden.Owners

            var garden = new Garden { Id = 1, Name = "Main Garden", CreatedAt = DateTime.UtcNow };
            garden.Owners.Add(farmer1); // Farmer 1 is the owner
            db.Gardens.Add(garden);

            db.Devices.AddRange(
                new Device { Id = 1, DeviceName = "Owned-1", MacAddress = "AA:BB:CC:DD:EE:21", UserId = 1, GardenId = 1, Status = "Active", CreatedAt = DateTime.UtcNow, LastSeen = DateTime.UtcNow },
                new Device { Id = 2, DeviceName = "Owned-2", MacAddress = "AA:BB:CC:DD:EE:22", UserId = 2, GardenId = 1, Status = "Active", CreatedAt = DateTime.UtcNow, LastSeen = DateTime.UtcNow });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "1");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Farmer");

        var response = await client.GetAsync("/api/garden/1/devices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        var data = payload.GetProperty("data");
        Assert.Equal(2, data.GetArrayLength()); // Farmer sees all devices in their owned garden
    }

    [Fact]
    public async Task GetGardenDevicesForbiddenForUnownedGarden()
    {
        await ResetDatabaseAsync(db =>
        {
            var farmer1 = new User { Id = 1, Username = "farmer-1", Email = "farmer1@test.local", PasswordHash = "hash", Role = "Farmer", CreatedAt = DateTime.UtcNow };
            var farmer2 = new User { Id = 2, Username = "farmer-2", Email = "farmer2@test.local", PasswordHash = "hash", Role = "Farmer", CreatedAt = DateTime.UtcNow };
            
            db.Users.Add(farmer2); // farmer1 is added implicitly via garden.Owners

            var garden = new Garden { Id = 1, Name = "Main Garden", CreatedAt = DateTime.UtcNow };
            garden.Owners.Add(farmer1); // Only Farmer 1 owns it
            db.Gardens.Add(garden);

            db.Devices.Add(new Device { Id = 1, DeviceName = "Owned-1", MacAddress = "AA:BB:CC:DD:EE:21", UserId = 1, GardenId = 1, Status = "Active", CreatedAt = DateTime.UtcNow, LastSeen = DateTime.UtcNow });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "2"); // Farmer 2 tries to access
        client.DefaultRequestHeaders.Add("X-Test-Role", "Farmer");

        var response = await client.GetAsync("/api/garden/1/devices");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetGardenDevicesReturnsAllForAdmin()
    {
        await ResetDatabaseAsync(db =>
        {
            db.Users.Add(new User { Id = 99, Username = "admin", Email = "admin@test.local", PasswordHash = "hash", Role = "Administrator", CreatedAt = DateTime.UtcNow });
            db.Gardens.Add(new Garden { Id = 1, Name = "Main Garden", CreatedAt = DateTime.UtcNow });
            db.Devices.AddRange(
                new Device { Id = 1, DeviceName = "Owned-1", MacAddress = "AA:BB:CC:DD:EE:21", UserId = 1, GardenId = 1, Status = "Active", CreatedAt = DateTime.UtcNow, LastSeen = DateTime.UtcNow },
                new Device { Id = 2, DeviceName = "Owned-2", MacAddress = "AA:BB:CC:DD:EE:22", UserId = 2, GardenId = 1, Status = "Active", CreatedAt = DateTime.UtcNow, LastSeen = DateTime.UtcNow });
        });

        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", "99");
        client.DefaultRequestHeaders.Add("X-Test-Role", "Administrator");

        var response = await client.GetAsync("/api/garden/1/devices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(2, payload.GetProperty("data").GetArrayLength());
    }

    private async Task ResetDatabaseAsync(Action<ApplicationDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        seed(db);
        await db.SaveChangesAsync();
    }
}
