using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using PMS.API.Attributes;

namespace PMS.API.Extensions;

/*PartitionedRateLimiter 5 minutes per user (IP address) for login attempts
User A → 5 login attempts/minute ✅
User B → 5 login attempts/minute ✅
User C → 5 login attempts/minute ✅
*/
public static class RateLimiterExtensions
{
    public static IServiceCollection AddApplicationRateLimiter(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            // Login + Reset Password: 5 requests/minute/IP
            options.AddPolicy("auth-5", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Register + Forgot Password: 3 requests/minute/IP
            options.AddPolicy("auth-3", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey:
                        httpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Global limiter: 100 requests/minute/IP
            options.GlobalLimiter =
                PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext =>
                    {
                        // Skip global limiter for designated auth endpoints
                        if (httpContext.GetEndpoint()?
                            .Metadata
                            .GetMetadata<SkipGlobalRateLimitAttribute>()
                            is not null)
                        {
                            return RateLimitPartition.GetNoLimiter(
                                "auth-endpoint");
                        }

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

        return services;
    }
}