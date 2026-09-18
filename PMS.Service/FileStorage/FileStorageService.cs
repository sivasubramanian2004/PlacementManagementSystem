using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.FileStorage
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _webRootPath;
        private readonly FileSettings _fileSettings;
        private readonly ILogger<FileStorageService> _logger;

        public FileStorageService(
            IWebHostEnvironment env,
            IOptions<FileSettings> fileSettings,
            ILogger<FileStorageService> logger)
        {
            _webRootPath = env.WebRootPath
                           ?? Path.Combine(env.ContentRootPath, "wwwroot");

            _fileSettings = fileSettings.Value;
            _logger = logger;
        }

        public async Task<FileUploadResult> UploadAsync(IFormFile file, string folder)
        {
            // ---------------- Validation ----------------

            if (file == null || file.Length == 0)
                throw new ArgumentException("No file was uploaded.");

            var maxBytes = _fileSettings.MaxFileSizeInMB * 1024L * 1024L;

            if (file.Length > maxBytes)
            {
                throw new ArgumentException($"File size exceeds the maximum allowed limit of " +
                     $"{_fileSettings.MaxFileSizeInMB} MB.");

            }
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!_fileSettings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))

            {
                throw new ArgumentException(
                    $"File type '{extension}' is not allowed. " +
                    $"Allowed types: " +
                    $"{string.Join(", ", _fileSettings.AllowedExtensions)}");
            }

            // ---------------- Folder ----------------

            var uploadFolder = Path.Combine(_webRootPath, _fileSettings.BasePath, folder);

            Directory.CreateDirectory(uploadFolder);

            // ---------------- File Name ----------------

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

            //physical path on your server: C:\MyProject\wwwroot\UploadedDocuments\Careers\abc123.jpg --full path

            var fullPath = Path.Combine(uploadFolder, uniqueFileName);
            // ---------------- Save File ----------------

            try
            {
                await using var stream = new FileStream(
                                        fullPath,
                                        FileMode.Create,
                                        FileAccess.Write,
                                        FileShare.None);


                await file.CopyToAsync(stream);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to save uploaded file. FileName: {FileName}, Folder: {Folder}", file.FileName, folder);

                throw new InvalidOperationException("Failed to save the uploaded file. Please try again.", ex);
            }
            // ---------------- Relative URL ----------------

            // relative path for /UploadedDocuments / Careers / abc123.jpg
            var relativePath = "/" + Path.Combine(_fileSettings.BasePath, folder, uniqueFileName).Replace("\\", "/");

            return new FileUploadResult
            {
                FileName = uniqueFileName,
                OriginalFileName = file.FileName,
                RelativePath = relativePath,
                FullPath = fullPath,
                FileSize = file.Length
            };
        }
        public Task DeleteAsync(string fullPath)
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
            return Task.CompletedTask;
        }
        public Task<string> GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return Task.FromResult(string.Empty);

            var cleanPath = relativePath
                .TrimStart('/')
                .Replace(
                    "/",
                    Path.DirectorySeparatorChar.ToString());

            var fullPath = Path.Combine(_webRootPath, cleanPath);

            return Task.FromResult(fullPath);
        }
    }
}
