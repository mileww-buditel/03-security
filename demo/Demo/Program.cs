using Demo.Data;
using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

using static Demo.Common.Constants;

var builder = WebApplication.CreateBuilder(args);

builder
    .Services
    .AddSingleton<HtmlSanitizer>()
    .AddOpenApi()
    .AddDbContext<ProductsDbContext>(
        options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")))
    .AddCors(options =>
    {
        // Broken: wildcard — any origin, any method, any header.
        // The browser will happily send cross-origin requests to this API
        // from any website in the world.
        options
            .AddPolicy(
                CorsPolicies.Broken,
                policy => policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader());

        // Correct: explicit allowlist — only trusted origins, only needed methods.
        // Any cross-origin request from an origin not in the list will be
        // rejected by the browser before it even reaches the controller.
        //options
        //    .AddPolicy(
        //        CorsPolicies.Correct,
        //        policy => policy
        //            .WithOrigins(
        //                "https://trusted-frontend.com",
        //                "http://localhost:5173")
        //            .WithMethods("GET", "POST")
        //            .WithHeaders("Authorization", "Content-Type"));
    })
    .AddRateLimiter(options =>
    {
        // Allows up to 5 requests per 10-second window, per IP.
        // Once the limit is hit, subsequent requests receive 429 Too Many Requests
        // until the window resets.
        //options
        //    .AddFixedWindowLimiter(
        //        RateLimiterPolicies.LoginFixedWindow,
        //        limiterOptions =>
        //        {
        //            limiterOptions.Window = TimeSpan.FromSeconds(10);
        //            limiterOptions.PermitLimit = 5;
        //            limiterOptions.QueueLimit = 0;
        //            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        //        });

        //options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    })
    .AddControllers();

var app = builder.Build();

var scope = app.Services.CreateScope();
var data = scope
    .ServiceProvider
    .GetRequiredService<ProductsDbContext>();

await data
    .Database
    .MigrateAsync(app.Lifetime.ApplicationStopping);

await DbSeeder.Seed(
    data,
    app.Lifetime.ApplicationStopping);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app
    .UseCors(CorsPolicies.Broken)
    //.UseCors(CorsPolicies.Correct)
    //.UseRateLimiter()
    .UseAuthorization();

app.MapControllers();
await app.RunAsync(app.Lifetime.ApplicationStopping);
