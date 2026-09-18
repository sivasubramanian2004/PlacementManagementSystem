using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using PMS.Core.DTOs.Students;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.Students
{
    public interface IStudentService
    {
        Task<StudentResponseDto> CreateStudentAsync(CreateStudentRequestDto dto, int createdBy);

        Task DeleteStudentAsync(int id, int deletedBy);

        Task<PagedResult<StudentResponseDto>> GetAllStudentAsync(StudentQueryParameters request);

        Task<StudentBasicDto> GetStudentByIdAsync(int id);

        Task<int> GetStudentId(int UserId);

        Task<StudentBasicDto> GetMyProfileAsync(int StudentId);

        Task UpdateStudentAsync(int id, UpdateStudentRequestDto dto, int updatedBy);
    }
}
