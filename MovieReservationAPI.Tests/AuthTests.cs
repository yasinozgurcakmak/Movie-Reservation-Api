using System.Net;
using System.Net.Http.Json;
using System.Text;
using BusinessLayer.Concrete;
using DataAccessLayer.Context;
using DataAccessLayer.Seed;
using DtoLayer.Auth;
using DtoLayer.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MovieReservationAPI.Tests.Infrastructure;

namespace MovieReservationAPI.Tests;

public class AuthTests : ApiTestBase
{
    public AuthTests(ApiFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Tokens_signed_with_another_algorithm_are_rejected_even_with_the_right_key()
    {
        var (user, _) = await RegisterVerifiedUserAsync();
        await using var scope = Factory.Services.CreateAsyncScope();
        var stamp = await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
            .Where(u => u.Id == user.Id).Select(u => u.SecurityStamp).SingleAsync();

        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = "MovieReservationAPI",
            Audience = "MovieReservationAPI.Client",
            Expires = DateTime.UtcNow.AddMinutes(10),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
                ["role"] = "User",
                [TokenService.SecurityStampClaimType] = stamp
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.JwtSecretKey)), SecurityAlgorithms.HmacSha384)
        });

        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient(token).GetAsync("/api/users/me")).StatusCode);
    }

    [Fact]
    public async Task Swagger_is_available_outside_development()
    {
        Assert.Equal(HttpStatusCode.OK, (await CreateClient().GetAsync("/swagger/index.html")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateClient().GetAsync("/openapi/v1.json")).StatusCode);
    }

    [Fact]
    public async Task Changing_the_password_revokes_old_tokens_and_returns_a_working_one()
    {
        var (_, oldToken) = await RegisterVerifiedUserAsync();

        var response = await CreateClient(oldToken).PutAsJsonAsync("/api/users/me/password",
            new ChangePasswordRequest(UserPassword, "New-Pass-456"));
        var newToken = (await response.Content.ReadFromJsonAsync<AuthResponse>())!.AccessToken;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient(oldToken).GetAsync("/api/users/me")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await CreateClient(newToken).GetAsync("/api/users/me")).StatusCode);
    }

    [Fact]
    public async Task Changing_a_role_revokes_the_users_existing_tokens()
    {
        var (user, userToken) = await RegisterVerifiedUserAsync();
        Assert.Equal(HttpStatusCode.OK, (await CreateClient(userToken).GetAsync("/api/users/me")).StatusCode);

        var promote = await CreateClient(await LoginAsAdminAsync())
            .PatchAsJsonAsync($"/api/users/{user.Id}/role", new UpdateUserRoleRequest("Editor"));

        Assert.Equal(HttpStatusCode.OK, promote.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await CreateClient(userToken).GetAsync("/api/users/me")).StatusCode);
    }

    [Fact]
    public async Task Wrong_current_password_is_rejected()
    {
        var (_, token) = await RegisterVerifiedUserAsync();

        var response = await CreateClient(token).PutAsJsonAsync("/api/users/me/password",
            new ChangePasswordRequest("Wrong-Pass-1", "New-Pass-456"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_is_rate_limited_per_client()
    {
        using var limitedFactory = Factory.WithWebHostBuilder(b => b.UseSetting("RateLimiting:AuthPermitLimit", "3"));
        var client = limitedFactory.CreateClient();

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 4; i++)
            statuses.Add((await client.PostAsJsonAsync("/api/auth/login", new LoginRequest("nobody@test.com", "Wrong-Pass-1"))).StatusCode);

        Assert.Equal([HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.Unauthorized, HttpStatusCode.TooManyRequests], statuses);
    }

    [Fact]
    public async Task Initializer_creates_the_admin_once_no_matter_how_often_it_runs()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await DbInitializer.InitializeAsync(context, ApiFactory.AdminEmail, ApiFactory.AdminPassword);
        await DbInitializer.InitializeAsync(context, ApiFactory.AdminEmail.ToUpperInvariant(), ApiFactory.AdminPassword);

        Assert.Equal(1, await context.Users.CountAsync(u => u.Email == ApiFactory.AdminEmail));
    }

    [Fact]
    public async Task Health_endpoint_reports_healthy_dependencies()
    {
        var response = await CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }
}
