using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace PMS.Core.Helpers
{
    public interface IUrlHelperService
    {
        string GetBaseUrl();
        string BuildFullUrl(string? relativePath);
    }

    public class UrlHelperService : IUrlHelperService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UrlHelperService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetBaseUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request
                ?? throw new InvalidOperationException("HttpContext is not available.");

            return $"{request.Scheme}://{request.Host}";
        }

        public string? BuildFullUrl(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            var baseUrl = GetBaseUrl();

            // Ensure exactly one slash between base URL and relative path
            return $"{baseUrl}/{relativePath.TrimStart('/')}";
        }
    }
}
