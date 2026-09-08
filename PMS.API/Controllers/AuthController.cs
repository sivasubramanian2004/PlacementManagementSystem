using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Core.Helpers;
using PMS.Service.Authentication;
namespace PMS.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        /*
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _authService.RegisterAsync(dto);

            var response = new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Registration successful",
                Data = result,
                Errors = null,
                StatusCode = 201
            };

            return StatusCode(201, response);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            var response = new ApiResponse<AuthResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = result,
                Errors = null,
                StatusCode = 200
            };

            return Ok(response);
        }
        */
    }

}