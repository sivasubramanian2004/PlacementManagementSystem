using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Core.DTOs.Auth;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using PMS.Service.Email;
using PMS.Service.TokenGenerator;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace PMS.Service.Authentication
{

        public class AuthService : IAuthService
        {
            private readonly IRepository<User> _userRepo;
            private readonly IJwtTokenGenerator _jwtGenerator;
            private readonly IEmailService _emailService;
            private readonly ILogger<AuthService> _logger;

            public AuthService(IRepository<User> userRepo, IJwtTokenGenerator jwtGenerator, IEmailService emailService, ILogger<AuthService> logger)
            {
                _userRepo = userRepo;
                _jwtGenerator = jwtGenerator;
                _emailService = emailService;
                _logger = logger;
            }

            public async Task<AuthResponseDto> RegisterAsync(AuthRequestDto dto)
            {
                // Check for existing email (excluding soft-deleted users)
                var existingUser = await _userRepo.TableNoTracking
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsDeleted != true);

                if (existingUser != null)
                    throw new InvalidOperationException("Email is already registered.");   // → 409 Conflict

                var user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    Role = UserRole.Student,             // ← always forced, never trust client input
                    CreatedDate = DateTime.UtcNow
                };

                var savedUser = await _userRepo.InsertAsync(user);
                _logger.LogInformation("User registered: {Email}, UserId: {UserId} successfully", savedUser.Email, savedUser.Id);

            var emailBody = $@"
                         <!DOCTYPE html>
                         <html>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>

<body style='margin:0;padding:20px;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;'>

<div style='max-width:500px;margin:0 auto;background:#ffffff;border-radius:12px;padding:30px;box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

    <div style='text-align:center;margin-bottom:25px;'>
        <h2 style='margin:0;color:#198754;'>
            Placement Cell
        </h2>
    </div>

    <div style='text-align:center;margin-bottom:25px;'>
        <div style='display:inline-block;
                    background:#e8f5e9;
                    color:#198754;
                    border:2px solid #198754;
                    border-radius:50%;
                    width:60px;
                    height:60px;
                    line-height:60px;
                    font-size:30px;
                    font-weight:bold;'>
            ✓
        </div>
    </div>

    <h3 style='margin-top:0;color:#333;text-align:center;'>
        Account Created Successfully
    </h3>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        Hello <strong>{savedUser.FirstName} {savedUser.LastName}</strong>,
    </p>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        Your account has been successfully created with the Placement Cell.
        You can now log in using your registered email address and password.
    </p>

    <div style='background:#f8f9fa;
                border-left:4px solid #198754;
                padding:15px;
                border-radius:6px;
                margin:25px 0;'>

        <p style='margin:0 0 8px;color:#444;font-size:14px;'>
            <strong>Email:</strong> {savedUser.Email}
        </p

    </div>

    <div style='background:#fff3cd;
                border-left:4px solid #ffc107;
                padding:15px;
                border-radius:6px;
                margin-bottom:25px;'>

        <p style='margin:0;color:#664d03;font-size:14px;line-height:1.6;'>
            🔐 <strong>Security Reminder:</strong>
            Never share your password with anyone.
        </p>

    </div>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        If you did not create this account, please contact the Placement Cell Team.
    </p>

    <hr style='border:none;border-top:1px solid #e5e5e5;margin:30px 0;'>

    <p style='margin:0;color:#666;font-size:14px;line-height:1.7;'>
        Regards,<br>
        <strong style='color:#198754;'>Placement Cell Team</strong>
    </p>

</div>

</body>
</html>";
            await _emailService.SendEmailAsync(savedUser.Email, "Account Created - Placement Cell", emailBody);
            return new AuthResponseDto
            {
                    UserId = savedUser.Id,
                    FirstName = savedUser.FirstName,
                    LastName = savedUser.LastName,
                    Email = savedUser.Email,
                    Role = savedUser.Role.ToString(),

            };
        }

            public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
            {
                var user = await _userRepo.TableNoTracking
                    .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsDeleted != true);

                if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    throw new UnauthorizedAccessException("Invalid email or password.");   // → 404 (deliberately vague — don't reveal WHICH part was wrong)
/*
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                    throw new UnauthorizedAccessException("Invalid email or password.");   // same vague message — security best practice
*/
                if (user.IsActive != true)
                    throw new UnauthorizedAccessException("Your account has been disabled. Contact admin.");   // → 401

                var (token, expiresAt) = _jwtGenerator.GenerateToken(user.Id, user.Email, user.Role, user.FirstName, user.LastName);

                _logger.LogInformation("user logged in: {Email}, UserId: {UserId} successfully", user.Email, user.Id);

                return new LoginResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    Role = user.Role,
                    Token = token,
                    ExpiresAt = expiresAt
                };
            }

        public async Task UpdateAsync(int id, AuthUpdateDto dto)
        {
            var user = await _userRepo.Table
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.FirstName))
            {
                user.FirstName = dto.FirstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.LastName))
            {
                user.LastName = dto.LastName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                user.PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            if (dto.Role.HasValue) { 
              
                user.Role=dto.Role.Value;
            
            }

            user.UpdatedDate = DateTime.UtcNow;
            user.UpdatedBy = id;

            await _userRepo.UpdateAsync(user);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            var user = await _userRepo.Table
                 .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsDeleted != true);
            if (user == null)
                throw new KeyNotFoundException("No account registered with Email.");
            var OtpCode = new Random().Next(100000, 999999).ToString();
            user.OtpCode = OtpCode;
            user.OtpExpiryTime = DateTime.UtcNow.AddMinutes(5);
            user.OtpIsUsed = false;
            await _userRepo.UpdateAsync(user);
            _logger.LogInformation("Otp Generated successfully with this registered mail {Email}", dto.Email);
            var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>

<body style='margin:0;padding:20px;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;'>

<div style='max-width:500px;margin:0 auto;background:#ffffff;border-radius:12px;padding:30px;box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

    <div style='text-align:center;margin-bottom:25px;'>
        <h2 style='margin:0;color:#198754;'>
            Placement Cell
        </h2>
    </div>

    <h3 style='margin-top:0;color:#333;'>
        Password Reset Request
    </h3>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        Hello,
    </p>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        We received a request to reset your password. Please use the One-Time Password (OTP) below to continue.
    </p>

    <div style='text-align:center;margin:35px 0;'>

        <div style='display:inline-block;
                    background:#e8f5e9;
                    color:#198754;
                    border:2px dashed #198754;
                    border-radius:10px;
                    padding:18px 30px;
                    font-size:34px;
                    font-weight:bold;
                    letter-spacing:8px;'>

            {OtpCode}

        </div>

    </div>

    <div style='background:#f8f9fa;
                border-left:4px solid #198754;
                padding:15px;
                border-radius:6px;
                margin-bottom:25px;'>

        <p style='margin:0;color:#444;font-size:14px;'>
            ⏳ <strong>OTP Validity:</strong> 5 Minutes
        </p>

    </div>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        <strong>Security Tips:</strong>
    </p>

    <ul style='color:#555;font-size:15px;line-height:1.8;padding-left:20px;'>
        <li>Never share this OTP with anyone.</li>
        <li>Our team will never ask for your OTP.</li>
        <li>If you didn't request a password reset, simply ignore this email.</li>
    </ul>

    <hr style='border:none;border-top:1px solid #e5e5e5;margin:30px 0;'>

    <p style='margin:0;color:#666;font-size:14px;line-height:1.7;'>
        Regards,<br>
        <strong style='color:#198754;'>Placement Cell Team</strong>
    </p>

</div>

</body>
</html>";

            await _emailService.SendEmailAsync(dto.Email, "Password Reset OTP - Placement Cell", emailBody);

        }
        public async Task ResetPasswordAsync(ResetPasswordDto dto)
        {

            var user = await _userRepo.Table
                     .FirstOrDefaultAsync(u => u.Email == dto.Email && u.IsDeleted != true);
            if (user == null)
                throw new KeyNotFoundException("No account registered with Email.");
            if (user.OtpCode != dto.OtpCode || user.OtpIsUsed == true)
                throw new ArgumentException("Invalid OTP.");
            if (user.OtpExpiryTime == null || user.OtpExpiryTime < DateTime.UtcNow)
                throw new ArgumentException("OTP has expired. Please request a new one.");
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            // Mark OTP as used — prevent reuse
            user.OtpIsUsed = true;
            user.OtpCode = null;
            user.OtpExpiryTime = null;
            await _userRepo.UpdateAsync(user);
            _logger.LogInformation("Password Changed Successfully with mail id {Email}", dto.Email);
            var emailBody = @"
<!DOCTYPE html>
<html>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
</head>

<body style='margin:0;padding:20px;background:#f3f4f6;font-family:Arial,Helvetica,sans-serif;'>

<div style='max-width:500px;margin:0 auto;background:#ffffff;border-radius:12px;padding:30px;box-shadow:0 4px 12px rgba(0,0,0,0.08);'>

    <div style='text-align:center;margin-bottom:25px;'>
        <h2 style='margin:0;color:#198754;'>
            Placement Cell
        </h2>
    </div>

    <div style='text-align:center;margin-bottom:25px;'>
        <div style='display:inline-block;
                    width:70px;
                    height:70px;
                    line-height:70px;
                    border-radius:50%;
                    background:#e8f5e9;
                    color:#198754;
                    font-size:36px;
                    font-weight:bold;'>
            ✓
        </div>
    </div>

    <h3 style='text-align:center;color:#198754;margin-top:0;'>
        Password Changed Successfully
    </h3>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        Hello,
    </p>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        Your password has been changed successfully. Your account is now secured with the new password.
    </p>

    <div style='background:#e8f5e9;
                border-left:4px solid #198754;
                padding:15px;
                border-radius:6px;
                margin:25px 0;'>

        <p style='margin:0;color:#444;font-size:14px;'>
            <strong>Security Notice:</strong><br>
            If you made this change, no further action is required.
        </p>

    </div>

    <p style='color:#555;font-size:15px;line-height:1.7;'>
        <strong>Didn't change your password?</strong><br>
        If you believe this password change was unauthorized, please contact your administrator or reset your password immediately.
    </p>

    <hr style='border:none;border-top:1px solid #e5e5e5;margin:30px 0;'>

    <p style='margin:0;color:#666;font-size:14px;line-height:1.7;'>
        Regards,<br>
        <strong style='color:#198754;'>Placement Cell Team</strong>
    </p>

</div>

</body>
</html>";
            await _emailService.SendEmailAsync(dto.Email, "Password Changed Successfully - Placement Cell", emailBody);

        }

        public async Task<PagedResult<UserBasicDto>> GetAllUsersAsync( UserQueryParameters request)
        {
            var query =  _userRepo.TableNoTracking.Where(u => !u.IsDeleted);

            //Global Searching across multiple fields such as firstname, lastname, email, role
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();

                query = query.Where(u =>
                        u.FirstName.Contains(searchTerm) ||
                        u.LastName.Contains(searchTerm) ||
                        u.Email.Contains(searchTerm));
                   
            }
            /*searching
            if (!string.IsNullOrWhiteSpace(request.FirstName))
            {
                var firstName = request.FirstName.Trim();
                query = query.Where(e => e.FirstName.Contains(firstName));
            }

            if (!string.IsNullOrWhiteSpace(request.LastName))
            {
                var lastName = request.LastName.Trim();
                query = query.Where(e => e.LastName.Contains(lastName));
            }
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                var email = request.Email.Trim();
                query = query.Where(e => e.Email.Contains(email));
            }
            */
            //filtering
            if (request.Role.HasValue)
            {
                var role = request.Role.Value;
                query = query.Where(u => u.Role == role);
            }
            if (request.IsActive.HasValue)
            {
                query = query.Where(e => e.IsActive == request.IsActive.Value);
            }
            //Created Date From
            if (request.CreatedFrom.HasValue)
            {
                var fromDate = request.CreatedFrom.Value
                    .ToDateTime(TimeOnly.MinValue);

                query = query.Where(c =>
                    c.CreatedDate >= fromDate);
            }

            // Created Date To
            if (request.CreatedTo.HasValue)
            {
                var toDate = request.CreatedTo.Value
                    .AddDays(1)
                    .ToDateTime(TimeOnly.MinValue);

                query = query.Where(c =>
                    c.CreatedDate < toDate);
            }

            //sorting using dictionary
            var sortOptions = new Dictionary<string, Expression<Func<User, object?>>>
            {
                ["firstname"] = e => e.FirstName,
                ["lastname"] = e => e.LastName,
                ["email"] = e => e.Email,
                ["role"] = e => e.Role,
                ["createddate"] = e => e.CreatedDate
            };

            query = query.ApplySorting(request, sortOptions, defaultSort: e => (object?)e.Id);

            //Mapster projection to DTO
            var resultQuery = query.ProjectToType<UserBasicDto>();

            return await resultQuery.ToPagedResultAsync(request);
        }

    }
}


