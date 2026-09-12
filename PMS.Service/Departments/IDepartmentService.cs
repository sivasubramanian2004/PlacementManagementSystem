using System;
using System.Collections.Generic;
using System.Text;
using PMS.Core.DTOs.Departments;

namespace PMS.Service.Departments
{
    public interface IDepartmentService
    {
        Task<DepartmentResponseDto> InsertAsync(CreateDepartmentRequestDto dto, int CreatedBy);
        Task DeleteAsync(int id, int DeletedBy);
    }
}
