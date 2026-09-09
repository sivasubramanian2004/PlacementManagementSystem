using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Core.Helpers;
using PMS.Core.DTOs.Auth;
using PMS.Service.Authentication;
namespace PMS.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
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
        [Authorize]
        [HttpPut("Update-Auth/{id:int}")]
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

        [AllowAnonymous]
        [HttpPost("Forget-Password")]
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
        [AllowAnonymous]
        [HttpPost("Reset-Password")]
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
    }

}