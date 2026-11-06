using BusinessLayer.Options;
using DataAccessLayer.Context;
using DataAccessLayer.Seed;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using MovieReservationAPI.ExceptionHandling;
using MovieReservationAPI.Extensions;
using MovieReservationAPI.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecurityTransformer>();
    options.AddOperationTransformer<BearerSecurityTransformer>();
});
builder.Services.AddControllers();
builder.Services.AddDataAccess(builder.Configuration);
builder.Services.AddBusiness(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiRateLimiting(builder.Configuration);
builder.Services.AddApiHealthChecks();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var admin = scope.ServiceProvider.GetRequiredService<IOptions<AdminSettings>>().Value;
    await DbInitializer.InitializeAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>(), admin.Email, admin.Password);
}

// Behind a reverse proxy (e.g. Coolify/Traefik): trust X-Forwarded-* so scheme, client IP and prefix are correct.
var forwardedOptions = new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.All };
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

// Serve the app under a sub-path when PathBase is set (e.g. /movie-reservation-api).
var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
    app.UsePathBase(pathBase);

app.UseExceptionHandler();
app.UseStatusCodePages();

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers.XFrameOptions = "DENY";
    context.Response.Headers["Referrer-Policy"] = "no-referrer";
    await next();
});

// Swagger is served in every environment so the API stays explorable after deployment (e.g. in Docker).
app.MapOpenApi();
app.UseSwaggerUI(options => options.SwaggerEndpoint("../openapi/v1.json", "MovieReservationAPI v1"));

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
