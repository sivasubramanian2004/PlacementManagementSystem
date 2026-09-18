using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PMS.Core.DTOs.Auth;
using PMS.Core.DTOs.Students;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Service.Students;
namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : BaseController
    {
        private readonly ILogger<StudentController> _logger;
        private readonly IStudentService _studentService;
        public StudentController(ILogger<StudentController> logger, IStudentService studentService)
        {
            _logger = logger;
            _studentService = studentService;
        }
        [HttpPost("Create-Student")]
        [Authorize]
        public async Task<IActionResult> CreateStudent([FromForm] CreateStudentRequestDto dto)
        {
            // Implementation for creating a student
            var createdBy = GetCurrentUserId();
            var result = await _studentService.CreateStudentAsync(dto, createdBy);
            var response = new ApiResponse<StudentResponseDto>
            {
                Success = true,
                Message = "Profile Created successfully",
                Data = result,
                Errors = null,
                StatusCode = 201
            };
            return StatusCode(201, response);

        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var deletedBy = GetCurrentUserId();
            await _studentService.DeleteStudentAsync(id, deletedBy);
            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Profile deleted successfully",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }
        [HttpGet("GetAll-Student")]
        public async Task<IActionResult> GetAllStudents([FromQuery] StudentQueryParameters request)
        {
            var result = await _studentService.GetAllStudentAsync(request);
            var response = new ApiResponse<PagedResult<StudentResponseDto>>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = result,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }

        [HttpGet("Get-Student/{id:int}")]
        public async Task<IActionResult> GetStudent(int id)
        {
            var result = await _studentService.GetStudentByIdAsync(id);
            var response = new ApiResponse<StudentBasicDto>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = result,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }

        [HttpGet("Get-MyProfile")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();
            var StudentId = await _studentService.GetStudentId(userId);
            var result = await _studentService.GetMyProfileAsync(StudentId);
            var response = new ApiResponse<StudentBasicDto>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = result,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }

        [HttpPatch]
        [Route("update-Student/{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateStudent(int id, [FromForm] UpdateStudentRequestDto dto)
        {
            var updatedBy = GetCurrentUserId();
            await _studentService.UpdateStudentAsync(id, dto, updatedBy);
            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Profile updated successfully",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);

        }

    }
}
