using System.Net;
using System.Net.Http.Json;
using DataAccessLayer.Context;
using DtoLayer.Genres;
using DtoLayer.Halls;
using DtoLayer.Showtimes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MovieReservationAPI.Tests.Infrastructure;

namespace MovieReservationAPI.Tests;

public class CatalogTests : ApiTestBase
{
    public CatalogTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Concurrent_overlapping_showtimes_in_one_hall_let_exactly_one_succeed()
    {
        var adminToken = await LoginAsAdminAsync();
        var hall = await CreateHallAsync(adminToken);
        var movie = await CreateMovieAsync(adminToken);
        var start = DateTimeOffset.UtcNow.AddDays(10);

        var responses = await Task.WhenAll(Enumerable.Range(0, 6).Select(i =>
            CreateClient(adminToken).PostAsJsonAsync("/api/showtimes", new ShowtimeRequest(movie.Id, hall.Id, start.AddMinutes(i * 5), 100m))));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.Created),
            r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
    }

    [Fact]
    public async Task Updating_a_hall_with_a_stale_version_is_rejected()
    {
        var adminToken = await LoginAsAdminAsync();
        var client = CreateClient(adminToken);
        var hall = await CreateHallAsync(adminToken);

        var first = await client.PutAsJsonAsync($"/api/halls/{hall.Id}",
            new HallRequest(hall.Name + "a", hall.RowCount, hall.SeatsPerRow, hall.Version));
        var second = await client.PutAsJsonAsync($"/api/halls/{hall.Id}",
            new HallRequest(hall.Name + "b", hall.RowCount, hall.SeatsPerRow, hall.Version));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Catalog_listing_reflects_a_rename_immediately()
    {
        var adminToken = await LoginAsAdminAsync();
        var client = CreateClient(adminToken);
        var created = await client.PostAsJsonAsync("/api/genres", new GenreRequest($"Genre {Guid.NewGuid():N}"[..30]));
        var genre = (await created.Content.ReadFromJsonAsync<GenreResponse>())!;
        await client.GetAsync("/api/genres"); // warms the cache

        await client.PutAsJsonAsync($"/api/genres/{genre.Id}", new GenreRequest(genre.Name + "x"));

        var genres = await client.GetFromJsonAsync<List<GenreResponse>>("/api/genres");
        Assert.Contains(genres!, g => g.Name == genre.Name + "x");
    }

    [Fact]
    public async Task Admin_changes_are_written_to_the_audit_log_with_the_acting_user()
    {
        var adminToken = await LoginAsAdminAsync();
        var hall = await CreateHallAsync(adminToken);

        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var adminId = await context.Users.Where(u => u.Email == ApiFactory.AdminEmail).Select(u => u.Id).SingleAsync();
        var entry = await context.AuditLogs.SingleAsync(a => a.EntityName == "Hall" && a.EntityId == hall.Id.ToString());

        Assert.Equal("Added", entry.Action);
        Assert.Equal(adminId, entry.UserId);
        Assert.Contains(hall.Name, entry.Changes);
    }
}
