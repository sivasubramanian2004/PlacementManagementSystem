using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PMS.Core.Helpers;

namespace PMS.Service.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly FileSettings _fileSettings;
    private readonly ILogger<FileStorageService> _logger;
    private readonly string _bucketName;

    public FileStorageService(
        Supabase.Client supabaseClient,
        IOptions<FileSettings> fileSettings,
        IConfiguration configuration,
        ILogger<FileStorageService> logger)
    {
        _supabaseClient = supabaseClient;
        _fileSettings = fileSettings.Value;
        _logger = logger;

        _bucketName = configuration["Supabase:BucketName"]
            ?? throw new InvalidOperationException(
                "Supabase bucket name is missing.");
    }

    public async Task<FileUploadResult> UploadAsync(
        IFormFile file,
        string folder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file was uploaded.");

        var maxBytes = _fileSettings.MaxFileSizeInMB * 1024L * 1024L;

        if (file.Length > maxBytes)
        {
            throw new ArgumentException(
                $"File size exceeds {_fileSettings.MaxFileSizeInMB} MB.");
        }

        var extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (!_fileSettings.AllowedExtensions.Contains(
                extension,
                StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                $"File type '{extension}' is not allowed.");
        }

        // Generate a unique name; never trust the original filename for storage.
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";

        var safeFolder = folder
            .Replace("\\", "/")
            .Trim('/');

        var storagePath = $"{safeFolder}/{uniqueFileName}";

        var temporaryFile = Path.Combine(
            Path.GetTempPath(),
            $"{Guid.NewGuid():N}{extension}");

        try
        {
            await using (var output = new FileStream(
                temporaryFile,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
            {
                await file.CopyToAsync(output);
            }

            await _supabaseClient.Storage
                .From(_bucketName)
                .Upload(
                    temporaryFile,
                    storagePath,
                    new Supabase.Storage.FileOptions
                    {
                        ContentType = file.ContentType,
                        Upsert = false
                    });

            _logger.LogInformation(
                "File uploaded to Supabase. StoragePath: {StoragePath}",
                storagePath);

            return new FileUploadResult
            {
                FileName = uniqueFileName,
                OriginalFileName = Path.GetFileName(file.FileName),
                RelativePath = storagePath,
                FileSize = file.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Supabase upload failed for {FileName}",
                file.FileName);

            throw;
        }
        finally
        {
            if (File.Exists(temporaryFile))
                File.Delete(temporaryFile);
        }
    }

    public async Task DeleteAsync(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("Storage path is required.");

        await _supabaseClient.Storage
            .From(_bucketName)
            .Remove(new List<string> { storagePath });
    }

    public async Task<string> GetSignedUrlAsync(string storagePath)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
            throw new ArgumentException("Storage path is required.");

        // URL is valid for 60 seconds.
        return await _supabaseClient.Storage
            .From(_bucketName)
            .CreateSignedUrl(storagePath, 1800);
    }
}