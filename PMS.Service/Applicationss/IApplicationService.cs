using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using PMS.Core.DTOs.Applications;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.Applicationss
{
    public interface IApplicationService
    {
        Task<ApplicationResponseDto> CreateAsync(CreateApplicationRequestDto dto, int UserId);

        Task<ApplicationDto> GetApplicationById(int id);

        Task<List<ApplicationDto>> GetMyApplication(int UserId);

        Task<PagedResult<ApplicationResponseDto>> GetAllApplication(ApplicationQueryParameters request);

        Task<byte[]> DownloadApplicationsAsync(ApplicationQueryParameters request);

        Task UpdateApplicationAsync(UpdateApplicationRequestDto dto,int id, int UpdatedBy);

        Task DeleteApplicationAsync(int id, int DeletedBy);
    }
}
