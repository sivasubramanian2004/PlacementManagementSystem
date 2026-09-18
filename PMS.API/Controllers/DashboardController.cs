using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Core.DTOs.Dashboard;
using PMS.Core.Helpers;
using PMS.Service.Dashboard;
namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetDashboard(
            [FromQuery] DashboardQueryParameters request)
        {
            var result = await _dashboardService.GetDashboardAsync(request);


            var response = new ApiResponse<Object>
            {
                Success = true,
                Message = "Dashboard Data Fetched Successfully",
                Data = result,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }
    }
}
