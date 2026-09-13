using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
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
    public class StudentController:BaseController
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
                Message = "Profile retrieved successfully",
                Data = result,
                Errors = null,
                StatusCode = 201
            };
            return StatusCode(201, response);

        }
    }
}
