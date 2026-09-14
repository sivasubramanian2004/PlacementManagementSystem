using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http.HttpResults;
using PMS.Core.DTOs.Companies;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Service.Companies
{
    public interface ICompanyService
    {
        Task<CompanyResponseDto> InsertAsync(CreateCompanyRequestDto dto, int CreatedBy);

        Task<PagedResult<CompanyBasicDto>> GetCompanyAsnyc(CompanyQueryParameters request);

        Task UpdateAsync(int id, UpdateCompanyRequestDto dto, int UpdatedBy);

        Task DeleteAsync(int id, int deletedBy);
    }
}
