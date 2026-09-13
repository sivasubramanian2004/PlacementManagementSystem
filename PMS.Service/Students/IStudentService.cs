using System;
using System.Collections.Generic;
using System.Text;
using PMS.Core.DTOs.Students;

namespace PMS.Service.Students
{
    public interface IStudentService
    {
        Task<StudentResponseDto> CreateStudentAsync(CreateStudentRequestDto dto, int createdBy);
    }
}
