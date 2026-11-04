using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using BusinessLayer.Common;
using BusinessLayer.Concrete;
using BusinessLayer.Interfaces;
using BusinessLayer.Options;
using BusinessLayer.Validators.Auth;
using DataAccessLayer.Auditing;
using DataAccessLayer.Context;
using DataAccessLayer.Repositories.Concrete;
using DataAccessLayer.Repositories.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MovieReservationAPI.Auditing;
using MovieReservationAPI.BackgroundServices;
using MovieReservationAPI.HealthChecks;
using MovieReservationAPI.RateLimiting;
using StackExchange.Redis;

namespace MovieReservationAPI.Extensions;

public static class ServiceCollectionExtensions
{
    private const int MinimumJwtKeyBytes = 32;

    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuditUserAccessor, HttpAuditUserAccessor>();
        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
            .AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>()));
        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis")!);
            options.ConfigurationOptions.BacklogPolicy = BacklogPolicy.FailFast;
            options.InstanceName = "MovieReservation:";
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
        services.AddScoped<IEmailVerificationTokenRepository, EmailVerificationTokenRepository>();
        services.AddScoped<IMovieRepository, MovieRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<IHallRepository, HallRepository>();
        services.AddScoped<IShowtimeRepository, ShowtimeRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();

        return services;
    }

    public static IServiceCollection AddBusiness(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        services.Configure<PasswordResetSettings>(configuration.GetSection(PasswordResetSettings.SectionName));
        services.Configure<EmailVerificationSettings>(configuration.GetSection(EmailVerificationSettings.SectionName));
        services.Configure<CinemaSettings>(configuration.GetSection(CinemaSettings.SectionName));
        services.Configure<AdminSettings>(configuration.GetSection(AdminSettings.SectionName));

        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IUserService, UserManager>();
        services.AddScoped<IPasswordResetService, PasswordResetManager>();
        services.AddScoped<IEmailVerificationService, EmailVerificationManager>();
        services.AddScoped<IMovieService, MovieManager>();
        services.AddScoped<IGenreService, GenreManager>();
        services.AddScoped<IHallService, HallManager>();
        services.AddScoped<IShowtimeService, ShowtimeManager>();
        services.AddScoped<IReservationService, ReservationManager>();
        services.AddScoped<IReportService, ReportManager>();

        services.AddSingleton<CatalogCache>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IEmailService, SmtpEmailService>();
        services.AddSingleton<IEmailQueue, EmailQueue>();
        services.AddHostedService<EmailDispatcher>();

        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtSettings.SectionName);
        services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing in configuration.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) || Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < MinimumJwtKeyBytes)
            throw new InvalidOperationException(
                $"Jwt:SecretKey must be at least {MinimumJwtKeyBytes} bytes. Set it with user-secrets or an environment variable.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = "role"
                };
                options.Events = new JwtBearerEvents
                {
                    // Rejects tokens issued before the user's password or role last changed.
                    OnTokenValidated = async context =>
                    {
                        var userId = context.Principal!.FindFirstValue(JwtRegisteredClaimNames.Sub);
                        var stamp = context.Principal!.FindFirstValue(TokenService.SecurityStampClaimType);
                        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();

                        if (!int.TryParse(userId, out var id) || stamp is null || !await userService.IsSecurityStampValidAsync(id, stamp))
                            context.Fail("The token has been revoked.");
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var authPermitLimit = configuration.GetValue("RateLimiting:AuthPermitLimit", 10);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
                return ValueTask.CompletedTask;
            };

            // Per client IP: slows down password guessing and stops anyone from flooding inboxes via the email endpoints.
            options.AddPolicy(RateLimitPolicies.Auth, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = authPermitLimit, Window = TimeSpan.FromMinutes(1) }));
        });

        return services;
    }

    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("postgres")
            .AddCheck<RedisHealthCheck>("redis");

        return services;
    }
}
