using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PMS.Core.Helpers;
using PMS.Data.Repositories;
using PMS.Data.UnitOfWork;
using PMS.Service.Applicationss;
using PMS.Service.Authentication;
using PMS.Service.Companies;
using PMS.Service.Dashboard;
using PMS.Service.Departments;
using PMS.Service.Email;
using PMS.Service.FileStorage;
using PMS.Service.PlacementDrives;
using PMS.Service.Students;
using PMS.Service.TokenGenerator;
using System.Text;

namespace PMS.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,

      IConfiguration configuration)
    {
        // =====================================
        // 1. REPOSITORY AND UNIT OF WORK
        // =====================================
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // =====================================
        // 2. BUSINESS SERVICES
        // =====================================
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IPlacementDriveService, PlacementDriveService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // =====================================
        // 3. SUPPORTING SERVICES
        // =====================================
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IUrlHelperService, UrlHelperService>();

        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddHttpContextAccessor();

        // =====================================
        // 4. CONFIGURATION SETTINGS
        // =====================================
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<FileSettings>(configuration.GetSection("FileSettings"));


        // =====================================
        // 5. JWT AUTHENTICATION
        // =====================================
        var jwtSettings = configuration.GetSection("JwtSettings");

        var secretKey = jwtSettings["SecretKey"]
                       ?? throw new InvalidOperationException("JWT SecretKey is missing.");

        services.AddAuthentication(options =>
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

        services.AddAuthorization();

        // =====================================
        // 6. SWAGGER + JWT
        // =====================================
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Placement Management System API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []

            });
        });

        // =====================================
        // 7. OUTPUT CACHE + HEALTH CHECKS
        // =====================================
        services.AddOutputCache();
        services.AddHealthChecks();

        return services;
    }

}