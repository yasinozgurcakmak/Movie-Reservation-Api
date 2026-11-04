using BusinessLayer.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace MovieReservationAPI.Tests.Infrastructure;

// Runs the real API against real PostgreSQL and Redis containers, because the behaviour under test
// (unique and exclusion constraints, row versions, cache invalidation) lives in those systems.
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin@cinema.com";
    public const string AdminPassword = "Admin-Test-123";
    public const string JwtSecretKey = "test-signing-key-that-is-long-enough-for-hmac-sha256";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder().WithImage("postgres:15-alpine").Build();
    private readonly RedisContainer _redis = new RedisBuilder().WithImage("redis:7-alpine").Build();

    public FakeEmailService Emails { get; } = new();

    public string PostgresConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask());
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", $"{_redis.GetConnectionString()},abortConnect=false");
        builder.UseSetting("Jwt:SecretKey", JwtSecretKey);
        builder.UseSetting("Admin:Email", AdminEmail);
        builder.UseSetting("Admin:Password", AdminPassword);
        builder.UseSetting("RateLimiting:AuthPermitLimit", "10000");

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IEmailService>(Emails);
        });
    }
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "api";
}
