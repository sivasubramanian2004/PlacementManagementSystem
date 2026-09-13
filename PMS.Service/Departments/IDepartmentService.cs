using System;
using System.Collections.Generic;
using System.Text;
using PMS.Core.Helpers;
using PMS.Core.DTOs.Departments;

namespace PMS.Service.Departments
{
    public interface IDepartmentService
    {
        Task<DepartmentResponseDto> InsertAsync(CreateDepartmentRequestDto dto, int CreatedBy);
        Task DeleteAsync(int id, int DeletedBy);
        Task<PagedResult<DepartmentResponseDto>> GetDepartmentsAsync(DepartmentQueryParameters request);

        Task UpdateAsync(int id, CreateDepartmentRequestDto dto, int UpdatedBy);
    }
}
