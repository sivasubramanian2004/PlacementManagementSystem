using DocumentFormat.OpenXml.Office2010.Excel;
using PMS.Core.DTOs.PlacementDrives;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.PlacementDrives
{
    public interface IPlacementDriveService
    {
        Task<PlacementDriveResponseDto> CreateAsync(CreatePlacementDriveRequestDto dto, int createdBy);
        Task UpdateAsync(int id, UpdatePlacementDriveRequestDto dto, int updatedBy);

        Task DeleteAsync(int id, int deletedBy);

        Task<PlacementDriveDto> GetPlacementDriveByIdAsync(int id);

        Task<PagedResult<PlacementDriveResponseDto>> GetAllPlacementDriveAsync(PlacementDriveQueryParameters request);
    }
}
