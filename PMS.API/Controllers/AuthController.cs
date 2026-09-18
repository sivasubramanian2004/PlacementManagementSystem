using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PMS.Core.DTOs.Auth;
using PMS.Core.Helpers;
using PMS.Service.Authentication;
namespace PMS.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {

        
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-3")]

        public async Task<IActionResult> Register([FromBody] AuthRequestDto dto)
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
        
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-5")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            var response = new ApiResponse<LoginResponseDto>
            {
                Success = true,
                Message = "Login successful",
                Data = result,
                Errors = null,
                StatusCode = 200
            };

            return Ok(response);
        }
        
        [HttpPatch("Update-Auth/{id:int}")]
        [Authorize]
        public async Task<IActionResult> Update(int id, [FromBody] AuthUpdateDto dto)
        {

            await _authService.UpdateAsync(id, dto);
            var response = new ApiResponse<object>
            {
                Success = true,
                Message = "User details updated successfully.",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }

        
        [HttpPost("Forget-Password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-3")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordDto dto)
        {
            await _authService.ForgotPasswordAsync(dto);
            var response = new ApiResponse<Object>
            {
                Success = true,
                Message = "Password OTP sent to Registered Mail Id",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }
        
        [HttpPost("Reset-Password")]
        [AllowAnonymous]
        [EnableRateLimiting("auth-5")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {

            await _authService.ResetPasswordAsync(dto);

            var response = new ApiResponse<Object>
            {
                Success = true,
                Message = "Password Reset Successfully",
                Data = null,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }
       
        
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParameters request)
        {
            var userId = GetCurrentUserId();
            var user = await _authService.GetAllUsersAsync(request);
            var response = new ApiResponse<PagedResult<UserBasicDto>>
            {
                Success = true,
                Message = "Profile retrieved successfully",
                Data = user,
                Errors = null,
                StatusCode = 200
            };
            return Ok(response);
        }
    }

}