using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtoLayer.Auth;
using DtoLayer.Halls;
using DtoLayer.Movies;
using DtoLayer.Showtimes;
using DtoLayer.Users;

namespace MovieReservationAPI.Tests.Infrastructure;

[Collection(ApiCollection.Name)]
public abstract class ApiTestBase
{
    protected const string UserPassword = "User-Pass-123";

    protected ApiTestBase(ApiFactory factory)
    {
        Factory = factory;
    }

    protected ApiFactory Factory { get; }

    protected static string UniqueEmail() => $"user-{Guid.NewGuid():N}@test.com";

    protected HttpClient CreateClient(string? accessToken = null)
    {
        var client = Factory.CreateClient();
        if (accessToken is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    protected async Task<string> LoginAsync(string email, string password)
    {
        var response = await CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!.AccessToken;
    }

    protected Task<string> LoginAsAdminAsync() => LoginAsync(ApiFactory.AdminEmail, ApiFactory.AdminPassword);

    protected async Task<(UserResponse User, string Email)> RegisterAsync()
    {
        var email = UniqueEmail();
        var response = await CreateClient().PostAsJsonAsync("/api/auth/register", new RegisterRequest("Test", "User", email, UserPassword));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return ((await response.Content.ReadFromJsonAsync<UserResponse>())!, email);
    }

    protected async Task<(UserResponse User, string Token)> RegisterVerifiedUserAsync()
    {
        var (user, email) = await RegisterAsync();
        var verificationEmail = await Factory.Emails.WaitForAsync(email, "Verify your email address");

        var response = await CreateClient().PostAsJsonAsync("/api/auth/verify-email",
            new VerifyEmailRequest(FakeEmailService.ExtractToken(verificationEmail)));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        return (user, await LoginAsync(email, UserPassword));
    }

    protected async Task<HallResponse> CreateHallAsync(string adminToken, int rows = 5, int seatsPerRow = 10)
    {
        var response = await CreateClient(adminToken).PostAsJsonAsync("/api/halls",
            new HallRequest($"Hall {Guid.NewGuid():N}"[..30], rows, seatsPerRow));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<HallResponse>())!;
    }

    protected async Task<MovieResponse> CreateMovieAsync(string adminToken)
    {
        var response = await CreateClient(adminToken).PostAsJsonAsync("/api/movies", new MovieRequest(
            $"Movie {Guid.NewGuid():N}", "A test movie.", "https://example.com/poster.jpg", 120, new DateOnly(2026, 1, 1), [1]));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<MovieResponse>())!;
    }

    protected async Task<ShowtimeResponse> CreateShowtimeAsync(string adminToken, int? hallId = null, int? movieId = null, DateTimeOffset? startTime = null)
    {
        hallId ??= (await CreateHallAsync(adminToken)).Id;
        movieId ??= (await CreateMovieAsync(adminToken)).Id;
        startTime ??= DateTimeOffset.UtcNow.AddDays(7);

        var response = await CreateClient(adminToken).PostAsJsonAsync("/api/showtimes",
            new ShowtimeRequest(movieId.Value, hallId.Value, startTime.Value, 100m));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<ShowtimeResponse>())!;
    }
}
