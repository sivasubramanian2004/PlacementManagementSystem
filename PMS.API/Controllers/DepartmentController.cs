using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PMS.Core.DTOs.Departments;
using PMS.Core.Helpers;
using PMS.Service.Departments;

namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class DepartmentController : BaseController
    {
        private readonly IDepartmentService _departmentService;
        public DepartmentController(IDepartmentService departmentService) => _departmentService = departmentService;

        [Authorize]
        [HttpPost("Add-Departments")]
        public async Task<IActionResult> Insert([FromBody] CreateDepartmentRequestDto dto)
        {
            var CreatedBy = GetCurrentUserId();
            var result=await _departmentService.InsertAsync(dto, CreatedBy);

            var response = new ApiResponse<DepartmentResponseDto>
            {
                Success = true,
                Message = "Department Added Successfully",
                Data = result,
                Errors = null,
                StatusCode = 201
            };
            return StatusCode(201, response);
        }


        [Authorize]
        [HttpDelete("Delete-Department/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {

            var DeletedBy = GetCurrentUserId();
            await _departmentService.DeleteAsync(id, DeletedBy);
            var response = new ApiResponse<Object>
            {

                Success = true,
                Message = "Department deleted Successfully",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }

        [Authorize]
        [HttpPut("update-Department/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateDepartmentRequestDto dto)
        {

            var UpdatedBy = GetCurrentUserId();
            await _departmentService.UpdateAsync(id, dto, UpdatedBy);
            var response = new ApiResponse<Object>
            {

                Success = true,
                Message = "Department Updated Successfully",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);

        }


        [Authorize]
        [HttpGet("Get-Department")]
        public async Task<IActionResult> Getdepartment([FromQuery]DepartmentQueryParameters request)
        {

            var result = await _departmentService.GetDepartmentsAsync(request);

            var response = new ApiResponse<PagedResult<DepartmentResponseDto>>
            {
                Success = true,
                Message = "Department Fetched Successfully",
                Data = result,
                Errors = null,
                StatusCode = 200

            };
            return Ok(response);

        }
    }
}