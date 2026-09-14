using Microsoft.AspNetCore.Http;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.FileStorage
{
    public interface IFileStorageService
    {
        Task<FileUploadResult> UploadAsync(IFormFile file, string folder);

        Task DeleteAsync(string fullPath);

        Task<string> GetFullPath(string relativePath);
    }
}
