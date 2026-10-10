using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PMS.API.Attributes;
using PMS.API.Middleware;
using PMS.Core.Validators;
using PMS.Data;
using Serilog;
using System.Text.Json.Serialization;
using PMS.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ============================
// 1. SERILOG CONFIGURATION
// ============================
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)   // ← ADD THIS LINE — appsettings.json padikkum
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)   // explicit safety override
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 10,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// ============================
// 2. DATABASE (EF Core + SQL Server)
// ============================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// ============================
// 3. Custom Services and Dependency Injection
// ============================

builder.Services.AddApplicationServices(builder.Configuration);

// ============================
// 4. Rate Limiter
// ============================
builder.Services.AddApplicationRateLimiter();

// 5. Fluent validation for DTOs
builder.Services.AddValidatorsFromAssemblyContaining<AuthRequestDtoValidator>();

// 6. Controllers, validation filter, enum serialization
builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<ValidationFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });
  /*.AddJsonOptions(options =>
    {
           options.JsonSerializerOptions.DefaultIgnoreCondition =
               JsonIgnoreCondition.WhenWritingNull;
    });
 */


// ============================
// 6. CORS (allow frontend to call this API)
// ============================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

/* TEMPORARY DEBUG LINE
Console.WriteLine("========================================");
Console.WriteLine("ACTUAL CONNECTION STRING: " + app.Configuration.GetConnectionString("DefaultConnection"));
Console.WriteLine("========================================");
*/

// ============================
// MIDDLEWARE PIPELINE (ORDER MATTERS)
// ============================

app.UseGlobalExceptionHandler();   // must be FIRST — catches everything below it

/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
*/

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRateLimiter();
app.UseAuthentication();           // must come BEFORE Authorization
app.UseAuthorization();
app.UseOutputCache();
app.MapControllers();

app.MapGet("/health", () => Results.Ok("Healthy"))
   .WithName("HealthCheck")
   .WithTags("Health")
   .WithSummary("Checks whether the API is responding");
try
{
    Log.Information("Starting EMS API...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}

















