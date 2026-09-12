using DocumentFormat.OpenXml.Drawing.Charts;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using PMS.API.Attributes;
using PMS.API.Middleware;
using PMS.Core.Helpers;
using PMS.Core.Validators;
using PMS.Data;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using PMS.Data.UnitOfWork;
using PMS.Service.Authentication;
using PMS.Service.Email;
using PMS.Service.TokenGenerator;
using Serilog;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
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
// 3. JWT AUTHENTICATION
// ============================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});
builder.Services.AddAuthorization();


// Add services to the container.
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddHttpContextAccessor();   // add this, if not already there
// already covered via open-generic registration

//fluent validation for DTOs
builder.Services.AddValidatorsFromAssemblyContaining<AuthRequestDtoValidator>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

// ============================
// 5. CONTROLLERS + SWAGGER
// ============================
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Placement Management System API",
        Version = "v1"
    });

    // JWT Authentication
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});

/* Add JSON options to ignore null values in the response
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            JsonIgnoreCondition.WhenWritingNull;
    });
*/

//Keep DTO enum and configure JSON enum serialization
builder.Services.AddControllers()
     .AddJsonOptions(options =>
      {
          options.JsonSerializerOptions.Converters.Add(
              new JsonStringEnumConverter());
      });

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

/*PartitionedRateLimiter 5 minutes per user (IP address) for login attempts
User A → 5 login attempts/minute ✅
User B → 5 login attempts/minute ✅
User C → 5 login attempts/minute ✅
*/

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login + Reset Password
    options.AddPolicy("auth-5", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                          ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Register + Forgot Password
    options.AddPolicy("auth-3", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString()
                          ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Global limiter for normal APIs
    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            // Auth endpoints skip global limiter
            if (httpContext.GetEndpoint()?
                .Metadata
                .GetMetadata<SkipGlobalRateLimitAttribute>() is not null)
            {
                return RateLimitPartition.GetNoLimiter("auth-endpoint");
            }

            // Normal APIs → 100 requests/min/IP
            return RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
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

app.MapControllers();

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

















