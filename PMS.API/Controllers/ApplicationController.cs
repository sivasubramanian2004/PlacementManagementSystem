using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PMS.Core.DTOs.Applications;
using PMS.Core.Helpers;
using PMS.Service.Applicationss;
namespace PMS.API.Controllers
{
    public class ApplicationController : BaseController
    {

        private readonly IApplicationService _applicationService;

        public ApplicationController(IApplicationService applicationService)
        {



            _applicationService = applicationService;

        }

        [HttpPost("Create-Application")]
        [Authorize]
        public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequestDto dto)
        {

            var UserId = GetCurrentUserId();

            var result = await _applicationService.CreateAsync(dto, UserId);

            var response = new ApiResponse<ApplicationResponseDto>
            {

                Success = true,
                Message = "Application Submitted Succesffuly",
                Data = result,
                Errors = null,
                StatusCode = 201
            };

            return StatusCode(201, response);

        }

        [HttpGet("GetApplication{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetApplication(int id)
        {


            var result = await _applicationService.GetApplicationById(id);

            var response = new ApiResponse<ApplicationDto>
            {
                Success = true,

                Message = "Application retrieved Successfully",

                Data = result,

                Errors = null,

                StatusCode = 200

            };

            return Ok(response);


        }

        [HttpGet("Get-MyApplication")]
        [Authorize]
        public async Task<IActionResult> GetMyApplication()
        {


            var userId = GetCurrentUserId();

            var result = await _applicationService.GetMyApplication(userId);

            var response = new ApiResponse<List<ApplicationDto>>
            {

                Success = true,

                Message = "My Application retrieved Successfully",

                Data = result,

                Errors = null,

                StatusCode = 200


            };

            return Ok(response);


        }

        [HttpGet("GetAll-Applications")]
        [Authorize]
        public async Task<IActionResult> GetAllApplication([FromQuery]ApplicationQueryParameters request)
        {


            var result = await _applicationService.GetAllApplication(request);

            var response = new ApiResponse<PagedResult<ApplicationResponseDto>>
            {
                Success = true,

                Message = "Application retrieved Successfully",

                Data = result,

                Errors = null,

                StatusCode = 200

            };

            return Ok(response);


        }

        [HttpGet("Download-Applications")]
        [Authorize]
        public async Task<IActionResult> DownloadApplication([FromQuery] ApplicationQueryParameters request) {


            var file = await _applicationService.DownloadApplicationsAsync(request);


            var fileName = $"Applications_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";


            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

        }

        [HttpPatch]
        [Route("Update-Application/{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdatApplication(int id, [FromBody] UpdateApplicationRequestDto dto) {

            var UpdatedBy = GetCurrentUserId();

            await _applicationService.UpdateApplicationAsync(dto,id, UpdatedBy);

            var response = new ApiResponse<Object>
            {

                Success = true,
                Message = "Application Updated Successfully",
                Data = null,
                Errors = null,
                StatusCode = 200


            };

            return Ok(response);

        
        }

        [HttpDelete("Delete-Application")]
        [Authorize]
        public async Task<IActionResult> DeleteApplication(int id)
        {

            var DeletedBy = GetCurrentUserId();

            await _applicationService.DeleteApplicationAsync(id,DeletedBy);

            var response = new ApiResponse<PagedResult<ApplicationResponseDto>>
            {
                Success = true,

                Message = "Application retrieved Successfully",

                Data = null,

                Errors = null,

                StatusCode = 200

            };

            return Ok(response);

        }
    }
}
