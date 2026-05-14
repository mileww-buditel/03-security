using Exercise.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    // 1. TODO: Register HtmlSanitizer (using Ganss.Xss) with Singleton lifetime.
    .AddOpenApi()
    .AddDbContext<HrDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")))
    // 2. TODO: Add CORS.
    // - name: Exercise.Common.Constants.CorsPolicies.Correct
    // - origins: "https://trusted-frontend.com", "http://localhost:5173"
    // - methods: GET, POST
    // - headers: Authorization, Content-Type
    // 3. TODO: Add rate limiting.
    // - policy name: Exercise.Common.Constants.RateLimiterPolicies.LoginFixedWindow
    // - type: fixed window
    // - window: 10 seconds
    // - permit limit: 5 requests
    // - queue limit: 0
    // - rejection status code: 429 Too Many Requests
    .AddControllers();

var app = builder.Build();
var cancellationToken = app.Lifetime.ApplicationStopping;

if (!app.Environment.IsEnvironment("Testing"))
{
    var scope = app.Services.CreateScope();
    var data = scope
        .ServiceProvider
        .GetRequiredService<HrDbContext>();

    await data
        .Database
        .MigrateAsync(app.Lifetime.ApplicationStopping);

    await DbSeeder.Seed(
        data,
        app.Lifetime.ApplicationStopping);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 4. TODO: Add CORS middleware.
// - name: Exercise.Common.Constants.CorsPolicies.Correct
// 5. TODO: Add Rate Limiter middleware.
app.UseAuthorization();
app.MapControllers();

await app.RunAsync(cancellationToken);
