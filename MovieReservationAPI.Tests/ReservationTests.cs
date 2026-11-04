using System.Net;
using System.Net.Http.Json;
using DtoLayer.Reservations;
using DtoLayer.Showtimes;
using MovieReservationAPI.Tests.Infrastructure;

namespace MovieReservationAPI.Tests;

public class ReservationTests : ApiTestBase
{
    public ReservationTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Concurrent_reservations_for_the_same_seat_let_exactly_one_succeed()
    {
        var showtime = await CreateShowtimeAsync(await LoginAsAdminAsync());
        var users = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => RegisterVerifiedUserAsync()));

        var responses = await Task.WhenAll(users.Select(u =>
            CreateClient(u.Token).PostAsJsonAsync("/api/reservations", new ReservationRequest(showtime.Id, ["C5"]))));

        Assert.Equal(1, responses.Count(r => r.StatusCode == HttpStatusCode.Created));
        Assert.All(responses.Where(r => r.StatusCode != HttpStatusCode.Created),
            r => Assert.Equal(HttpStatusCode.Conflict, r.StatusCode));
    }

    [Fact]
    public async Task Unverified_user_cannot_reserve_until_email_is_verified()
    {
        var showtime = await CreateShowtimeAsync(await LoginAsAdminAsync());
        var (_, email) = await RegisterAsync();
        var client = CreateClient(await LoginAsync(email, UserPassword));

        var forbidden = await client.PostAsJsonAsync("/api/reservations", new ReservationRequest(showtime.Id, ["A1"]));
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.StatusCode);

        var verificationEmail = await Factory.Emails.WaitForAsync(email, "Verify your email address");
        await CreateClient().PostAsJsonAsync("/api/auth/verify-email",
            new DtoLayer.Auth.VerifyEmailRequest(FakeEmailService.ExtractToken(verificationEmail)));

        var created = await client.PostAsJsonAsync("/api/reservations", new ReservationRequest(showtime.Id, ["A1"]));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        await Factory.Emails.WaitForAsync(email, "Your reservation is confirmed");
    }

    [Fact]
    public async Task A_user_cannot_hold_more_than_ten_seats_of_one_showtime()
    {
        var showtime = await CreateShowtimeAsync(await LoginAsAdminAsync());
        var client = CreateClient((await RegisterVerifiedUserAsync()).Token);

        var tooMany = await client.PostAsJsonAsync("/api/reservations",
            new ReservationRequest(showtime.Id, Enumerable.Range(1, 10).Select(n => $"A{n}").Append("B1").ToList()));
        Assert.Equal(HttpStatusCode.BadRequest, tooMany.StatusCode);

        var first = await client.PostAsJsonAsync("/api/reservations",
            new ReservationRequest(showtime.Id, Enumerable.Range(1, 8).Select(n => $"A{n}").ToList()));
        Assert.Equal(HttpStatusCode.Created, first.StatusCode);

        var overQuota = await client.PostAsJsonAsync("/api/reservations", new ReservationRequest(showtime.Id, ["B1", "B2", "B3"]));
        Assert.Equal(HttpStatusCode.BadRequest, overQuota.StatusCode);
    }

    [Fact]
    public async Task Cancelling_a_showtime_cancels_its_reservations_and_notifies_customers()
    {
        var adminToken = await LoginAsAdminAsync();
        var showtime = await CreateShowtimeAsync(adminToken);
        var (user, userToken) = await RegisterVerifiedUserAsync();
        await CreateClient(userToken).PostAsJsonAsync("/api/reservations", new ReservationRequest(showtime.Id, ["A1"]));

        var cancel = await CreateClient(adminToken).DeleteAsync($"/api/showtimes/{showtime.Id}");

        Assert.Equal(HttpStatusCode.NoContent, cancel.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await CreateClient().GetAsync($"/api/showtimes/{showtime.Id}/seats")).StatusCode);
        var reservations = await CreateClient(userToken).GetFromJsonAsync<List<ReservationResponse>>("/api/reservations/my");
        Assert.Equal("Cancelled", Assert.Single(reservations!).Status);
        await Factory.Emails.WaitForAsync(user.Email, "Your showtime has been cancelled");
    }

    [Fact]
    public async Task A_showtime_with_reservations_cannot_be_changed()
    {
        var adminToken = await LoginAsAdminAsync();
        var showtime = await CreateShowtimeAsync(adminToken);
        await CreateClient((await RegisterVerifiedUserAsync()).Token)
            .PostAsJsonAsync("/api/reservations", new ReservationRequest(showtime.Id, ["A1"]));

        var update = await CreateClient(adminToken).PutAsJsonAsync($"/api/showtimes/{showtime.Id}",
            new ShowtimeRequest(showtime.MovieId, showtime.HallId, showtime.StartTime.AddHours(1), 120m));

        Assert.Equal(HttpStatusCode.Conflict, update.StatusCode);
    }

    [Fact]
    public async Task A_movie_with_upcoming_reservations_cannot_be_deleted()
    {
        var adminToken = await LoginAsAdminAsync();
        var showtime = await CreateShowtimeAsync(adminToken);
        await CreateClient((await RegisterVerifiedUserAsync()).Token)
            .PostAsJsonAsync("/api/reservations", new ReservationRequest(showtime.Id, ["A1"]));

        var delete = await CreateClient(adminToken).DeleteAsync($"/api/movies/{showtime.MovieId}");

        Assert.Equal(HttpStatusCode.Conflict, delete.StatusCode);
    }
}
