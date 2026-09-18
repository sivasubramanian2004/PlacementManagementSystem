using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Core.DTOs.Companies;
using PMS.Service.Companies;
using PMS.Core.Helpers;
using PMS.Service.Departments;
namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _companyService;
        public CompanyController(ICompanyService companyService) => _companyService = companyService;

        [Authorize]
        [HttpPost("Create-Company")]
        public async Task<IActionResult> Insert([FromBody] CreateCompanyRequestDto dto)
        {
            var CreatedBy = GetCurrentUserId();
            var result = await _companyService.InsertAsync(dto, CreatedBy);

            var response = new ApiResponse<CompanyResponseDto>
            {
                Success = true,
                Message = "Company created Successfully",
                Data = result,
                Errors = null,
                StatusCode = 201
            };
            return StatusCode(201, response);
        }

        [HttpGet("Get-Company")]
        public async Task<IActionResult> GetCompany([FromQuery] CompanyQueryParameters request) {

            var result = await _companyService.GetCompanyAsnyc(request);

            var response = new ApiResponse<PagedResult<CompanyBasicDto>>
            {

                Success = true,
                Message = "Companies Retrieved Succesfully",
                Data= result,
                Errors=null,
                StatusCode = 200

            };

            return Ok(response);
        
        }

        [HttpPatch("Update-Company/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromQuery] UpdateCompanyRequestDto dto) { 
        
           var UpdatedBy = GetCurrentUserId();
           await _companyService.UpdateAsync(id, dto, UpdatedBy);
            var response = new ApiResponse<Object>
            {
                Success = true,
                Message = "Company Updated Successfully",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);

        }

        [HttpDelete("Delete-Company/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var DeletedBy = GetCurrentUserId();
            await _companyService.DeleteAsync(id, DeletedBy);
            var response = new ApiResponse<Object>
            {
                Success = true,
                Message = "Company deleted Successfully",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }

    }

}
