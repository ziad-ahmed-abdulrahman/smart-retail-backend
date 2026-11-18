using System.Threading.RateLimiting;
using SmartRetail.Api.Helpers;
using SmartRetail.Api.Middlewares;
using SmartRetail.Application;
using SmartRetail.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();

// OpenAPI Document Generator (.NET 10)
builder.Services.AddOpenApi();

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("AuthPolicy", partitioner: httpContext =>
    {
        var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(remoteIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 15,
            Window = TimeSpan.FromSeconds(10),
            QueueLimit = 0
        });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        var response = new APIResponse<object>(429, "Too many requests. Please slow down.");
        await context.HttpContext.Response.WriteAsJsonAsync(response, token);
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "SmartRetail API v1");
    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowAllOrigins");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed or Sync Users and Passwords dynamically from appsettings.json
await SmartRetail.Infrastructure.Persistence.DbInitializer.SeedUsersAsync(app.Services, app.Configuration);

app.Run();
