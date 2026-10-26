using SmartRetail.Application;
using SmartRetail.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllers();

// OpenAPI Document Generator (.NET 10)
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Generates the OpenAPI schema endpoint: /openapi/v1.json
    app.MapOpenApi();

    // Swagger UI at: /swagger (and /swagger/index.html)
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "SmartRetail API v1");
        options.RoutePrefix = "swagger";
    });

    // Redirect root URL "/" directly to "/swagger"
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
