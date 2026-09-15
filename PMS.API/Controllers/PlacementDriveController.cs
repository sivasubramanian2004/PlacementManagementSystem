using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PMS.Core.DTOs.PlacementDrives;
using PMS.Core.Helpers;
using PMS.Service.PlacementDrives;
namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacementDriveController:BaseController
    {
        private readonly IPlacementDriveService _placementDriveService;

        public PlacementDriveController(IPlacementDriveService placementDriveService)
        {
            _placementDriveService = placementDriveService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePlacementDrive([FromBody] CreatePlacementDriveRequestDto dto) {


            var createdBy = GetCurrentUserId();
            var result=await _placementDriveService.CreateAsync(dto, createdBy);

            var response = new ApiResponse<PlacementDriveResponseDto>
            {

                Success = true,
                Message = "Placement drive created successfully",
                Data = result,
                Errors = null,
                StatusCode = 201,

            };

            return StatusCode(201, response);

        }

        [HttpPatch("Update-PlacementDrive/{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlacementDrive(int id, [FromBody] UpdatePlacementDriveRequestDto dto) {

            var updatedBy = GetCurrentUserId();
            await _placementDriveService.UpdateAsync(id, dto, updatedBy);

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Placement drive updated successfully",
                Data = null,
                Errors = null,
                StatusCode = 200,
            };

            return Ok(response);
        }

        [HttpDelete("Delete-PlacementDrive/{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePlacementDrive(int id) {
            // Implementation for deleting a placement drive
            var deletedBy = GetCurrentUserId(); 

            await _placementDriveService.DeleteAsync(id, deletedBy);

            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "Placement drive deleted successfully",
                Data = null,
                Errors = null,  
                StatusCode = 200,
            };  
            return Ok(response);
        }

        [HttpGet("GetAll-PlacementDrive")]
        [Authorize]
        public async Task<IActionResult> GetAllPlacementDrive([FromQuery]PlacementDriveQueryParameters request)
        {
            var result = await _placementDriveService.GetAllPlacementDriveAsync(request);
            var response = new ApiResponse<PagedResult<PlacementDriveResponseDto>>
            {
                Success = true,
                Message = "Placement drives retrieved successfully",
                Data = result,
                Errors = null,
                StatusCode = 200,
            };
            return Ok(response);

        }
        [HttpGet("Get-PlacementDrive/{id:int}")] 
        [Authorize]
        public async Task<IActionResult> GetPlacementDrive(int id) {
           
            var result = await _placementDriveService.GetPlacementDriveByIdAsync(id);

            var response = new ApiResponse<PlacementDriveDto>
            {
                Success = true,
                Message = "Placement drive retrieved successfully",
                Data = result,
                Errors = null,
                StatusCode = 200,
            };
            
            return Ok(response);
        }
    }
}
