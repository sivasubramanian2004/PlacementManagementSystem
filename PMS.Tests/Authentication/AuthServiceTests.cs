
    using Microsoft.Extensions.Logging;
    using MockQueryable;
    using MockQueryable.Moq;
    using Moq;
    using PMS.Core.DTOs.Auth;
using PMS.Core.Helpers;
using PMS.Data.Entities;
    using PMS.Data.Repositories;
    using PMS.Service.Authentication;
    using PMS.Service.Email;
    using PMS.Service.TokenGenerator;
    using Xunit;

    namespace PMS.Tests.Authentication;

    public class AuthServiceTests
    {
        private readonly Mock<IRepository<User>> _userRepoMock;
        private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;

        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IRepository<User>>();
            _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();
            _emailServiceMock = new Mock<IEmailService>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userRepoMock.Object,
                _jwtGeneratorMock.Object,
                _emailServiceMock.Object,
                _loggerMock.Object);
        }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var password = "Password@123";

        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false
        };

        var users = new List<User>
        {
        user
        };

        _userRepoMock
             .Setup(x => x.TableNoTracking)
             .Returns(users.BuildMock());

        var expectedExpiry = DateTime.UtcNow.AddHours(1);

        _jwtGeneratorMock
            .Setup(x => x.GenerateToken(
                user.Id,
                user.Email,
                user.Role,
                user.FirstName,
                user.LastName))
            .Returns(("fake-jwt-token", expectedExpiry));

        var dto = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        // Act
        var result = await _authService.LoginAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.UserId);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Role, result.Role);
        Assert.Equal("fake-jwt-token", result.Token);
        Assert.Equal(expectedExpiry, result.ExpiresAt);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var correctPassword = "Password@123";

        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(correctPassword),
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false
        };

        var users = new List<User>
        {
        user
        };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var dto = new LoginRequestDto
        {
            Email = user.Email,
            Password = "WrongPassword@123"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(dto));
    }
    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var users = new List<User>();

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var dto = new LoginRequestDto
        {
            Email = "unknown@gmail.com",
            Password = "Password@123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(dto));

        Assert.Equal("Invalid email or password.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInactiveUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var password = "Password@123";

        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Student,
            IsActive = false,
            IsDeleted = false
        };

        var users = new List<User>
        {
        user
        };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var dto = new LoginRequestDto
        {
            Email = user.Email,
            Password = password
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _authService.LoginAsync(dto));

        Assert.Equal(
            "Your account has been disabled. Contact admin.",
            exception.Message);
    }
    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldReturnAuthResponse()
    {
        // Arrange
        var dto = new AuthRequestDto
        {
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            Password = "Password@123"
        };

        var users = new List<User>();

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        _userRepoMock
            .Setup(x => x.InsertAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                user.Id = 1;
                return user;
            });

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(dto);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(1, result.UserId);
        Assert.Equal(dto.FirstName, result.FirstName);
        Assert.Equal(dto.LastName, result.LastName);
        Assert.Equal(dto.Email, result.Email);
        Assert.Equal("Student", result.Role);

        _userRepoMock.Verify(
            x => x.InsertAsync(It.Is<User>(u =>
                u.FirstName == dto.FirstName &&
                u.LastName == dto.LastName &&
                u.Email == dto.Email &&
                u.Role == UserRole.Student)),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                dto.Email,
                "Account Created - Placement Cell",
                It.IsAny<string>()),
            Times.Once);
    }
    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            FirstName = "Existing",
            LastName = "User",
            Email = "siva@gmail.com",
            IsDeleted = false
        };

        var users = new List<User>
    {
        existingUser
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var dto = new AuthRequestDto
        {
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            Password = "Password@123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(dto));

        Assert.Equal("Email is already registered.", exception.Message);

        // Make sure user was NOT inserted
        _userRepoMock.Verify(
            x => x.InsertAsync(It.IsAny<User>()),
            Times.Never);

        // Make sure email was NOT sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Fact]
    public async Task ForgotPasswordAsync_WithValidEmail_ShouldGenerateOtpAndSendEmail()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            IsDeleted = false,
            OtpIsUsed = true
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        _userRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User updatedUser) => updatedUser);

        _emailServiceMock
            .Setup(x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var dto = new ForgotPasswordDto
        {
            Email = "siva@gmail.com"
        };

        // Act
        await _authService.ForgotPasswordAsync(dto);

        // Assert
        Assert.NotNull(user.OtpCode);
        Assert.Equal(6, user.OtpCode.Length);
        Assert.False(user.OtpIsUsed);
        Assert.NotNull(user.OtpExpiryTime);
        Assert.True(user.OtpExpiryTime > DateTime.UtcNow);

        _userRepoMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u =>
                u.Email == dto.Email &&
                u.OtpCode != null &&
                u.OtpIsUsed == false &&
                u.OtpExpiryTime != null)),
            Times.Once);

        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                dto.Email,
                "Password Reset OTP - Placement Cell",
                It.IsAny<string>()),
            Times.Once);
    }
    [Fact]
    public async Task ForgotPasswordAsync_WithUnknownEmail_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var users = new List<User>();

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        var dto = new ForgotPasswordDto
        {
            Email = "unknown@gmail.com"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _authService.ForgotPasswordAsync(dto));

        Assert.Equal(
            "No account registered with Email.",
            exception.Message);

        // User should not be updated
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);

        // Email should not be sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Fact]
    public async Task ResetPasswordAsync_WithInvalidOtp_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword@123"),
            OtpCode = "123456",
            OtpExpiryTime = DateTime.UtcNow.AddMinutes(5),
            OtpIsUsed = false,
            IsDeleted = false
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        var dto = new ResetPasswordDto
        {
            Email = user.Email,
            OtpCode = "999999", // Wrong OTP
            NewPassword = "NewPassword@123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.ResetPasswordAsync(dto));

        Assert.Equal("Invalid OTP.", exception.Message);

        // Password should NOT be changed
        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                "OldPassword@123",
                user.PasswordHash));

        // Update should NOT happen
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);

        // Email should NOT be sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Fact]
    public async Task ResetPasswordAsync_WithExpiredOtp_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword@123"),
            OtpCode = "123456",
            OtpExpiryTime = DateTime.UtcNow.AddMinutes(-5), // Expired
            OtpIsUsed = false,
            IsDeleted = false
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        var dto = new ResetPasswordDto
        {
            Email = user.Email,
            OtpCode = "123456",
            NewPassword = "NewPassword@123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.ResetPasswordAsync(dto));

        Assert.Equal(
            "OTP has expired. Please request a new one.",
            exception.Message);

        // Password should NOT be changed
        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                "OldPassword@123",
                user.PasswordHash));

        // Database should NOT be updated
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);

        // Email should NOT be sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Fact]
    public async Task ResetPasswordAsync_WithUnknownEmail_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var users = new List<User>();

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        var dto = new ResetPasswordDto
        {
            Email = "unknown@gmail.com",
            OtpCode = "123456",
            NewPassword = "NewPassword@123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _authService.ResetPasswordAsync(dto));

        Assert.Equal(
            "No account registered with Email.",
            exception.Message);

        // Database should NOT be updated
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);

        // Email should NOT be sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Fact]
    public async Task ResetPasswordAsync_WithAlreadyUsedOtp_ShouldThrowArgumentException()
    {
        // Arrange
        var oldPassword = "OldPassword@123";

        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword),
            OtpCode = "123456",
            OtpExpiryTime = DateTime.UtcNow.AddMinutes(5),
            OtpIsUsed = true, // OTP already used
            IsDeleted = false
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        var dto = new ResetPasswordDto
        {
            Email = user.Email,
            OtpCode = "123456",
            NewPassword = "NewPassword@123"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _authService.ResetPasswordAsync(dto));

        Assert.Equal("Invalid OTP.", exception.Message);

        // Password should remain unchanged
        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                oldPassword,
                user.PasswordHash));

        // Database should NOT be updated
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);

        // Email should NOT be sent
        _emailServiceMock.Verify(
            x => x.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Fact]
    public async Task UpdateAsync_WithFirstNameAndLastName_ShouldUpdateUser()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "OldFirstName",
            LastName = "OldLastName",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword@123"),
            Role = UserRole.Student,
            IsDeleted = false
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        _userRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User updatedUser) => updatedUser);

        var dto = new AuthUpdateDto
        {
            FirstName = "NewFirstName",
            LastName = "NewLastName"
        };

        // Act
        await _authService.UpdateAsync(user.Id, dto);

        // Assert
        Assert.Equal("NewFirstName", user.FirstName);
        Assert.Equal("NewLastName", user.LastName);

        // Password should remain unchanged
        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                "OldPassword@123",
                user.PasswordHash));

        // Role should remain unchanged
        Assert.Equal(UserRole.Student, user.Role);

        // UpdateAsync should be called once
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u =>
                u.Id == user.Id &&
                u.FirstName == "NewFirstName" &&
                u.LastName == "NewLastName")),
            Times.Once);
    }
    [Fact]
    public async Task UpdateAsync_WithPasswordOnly_ShouldUpdatePassword()
    {
        // Arrange
        var oldPassword = "OldPassword@123";
        var newPassword = "NewPassword@123";

        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword),
            Role = UserRole.Student,
            IsDeleted = false
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        _userRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User updatedUser) => updatedUser);

        var dto = new AuthUpdateDto
        {
            Password = newPassword
        };

        // Act
        await _authService.UpdateAsync(user.Id, dto);

        // Assert

        // Password should be changed
        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                newPassword,
                user.PasswordHash));

        // FirstName should remain unchanged
        Assert.Equal("Siva", user.FirstName);

        // LastName should remain unchanged
        Assert.Equal("Subramanian", user.LastName);

        // Role should remain unchanged
        Assert.Equal(UserRole.Student, user.Role);

        // UpdateAsync should be called once
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u =>
                u.Id == user.Id &&
                BCrypt.Net.BCrypt.Verify(newPassword, u.PasswordHash) &&
                u.FirstName == "Siva" &&
                u.LastName == "Subramanian" &&
                u.Role == UserRole.Student)),
            Times.Once);
    }
    [Fact]
    public async Task UpdateAsync_WithRoleOnly_ShouldUpdateRole()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPassword@123"),
            Role = UserRole.Student,
            IsDeleted = false
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        _userRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User updatedUser) => updatedUser);

        var dto = new AuthUpdateDto
        {
            Role = UserRole.PlacementOfficer
        };

        // Act
        await _authService.UpdateAsync(user.Id, dto);

        // Assert

        // Role should be changed
        Assert.Equal(UserRole.PlacementOfficer, user.Role);

        // Other fields should remain unchanged
        Assert.Equal("Siva", user.FirstName);
        Assert.Equal("Subramanian", user.LastName);

        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                "OldPassword@123",
                user.PasswordHash));

        // UpdateAsync should be called once
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u =>
                u.Id == user.Id &&
                u.Role == UserRole.PlacementOfficer &&
                u.FirstName == "Siva" &&
                u.LastName == "Subramanian")),
            Times.Once);
    }
    [Fact]
    public async Task UpdateAsync_WithUnknownUser_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var users = new List<User>();

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        var dto = new AuthUpdateDto
        {
            FirstName = "NewName"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _authService.UpdateAsync(999, dto));

        Assert.Equal("User not found.", exception.Message);

        // Update should NOT happen
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<User>()),
            Times.Never);
    }
    [Fact]
    public async Task UpdateAsync_WithRoleAndPassword_ShouldUpdateBoth()
    {
        // Arrange
        var oldPassword = "OldPassword@123";
        var newPassword = "NewPassword@123";

        var user = new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(oldPassword),
            Role = UserRole.Student,
            IsDeleted = false
        };

        var users = new List<User>
    {
        user
    };

        _userRepoMock
            .Setup(x => x.Table)
            .Returns(users.BuildMock());

        _userRepoMock
            .Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync((User updatedUser) => updatedUser);

        var dto = new AuthUpdateDto
        {
            Password = newPassword,
            Role = UserRole.PlacementOfficer,
        };

        // Act
        await _authService.UpdateAsync(user.Id, dto);

        // Assert

        // Password should be changed
        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                newPassword,
                user.PasswordHash));

        // Old password should no longer work
        Assert.False(
            BCrypt.Net.BCrypt.Verify(
                oldPassword,
                user.PasswordHash));

        // Role should be changed
        Assert.Equal(UserRole.PlacementOfficer, user.Role);

        // Other fields should remain unchanged
        Assert.Equal("Siva", user.FirstName);
        Assert.Equal("Subramanian", user.LastName);

        // UpdateAsync should be called once
        _userRepoMock.Verify(
            x => x.UpdateAsync(It.Is<User>(u =>
                u.Id == user.Id &&
                u.Role == UserRole.PlacementOfficer)),
            Times.Once);
    }
    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnOnlyNonDeletedUsers()
    {
        // Arrange
        var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = new DateTime(2026, 1, 1)
        },
        new User
        {
            Id = 2,
            FirstName = "Kumar",
            LastName = "Raj",
            Email = "kumar@gmail.com",
            Role = UserRole.Admin,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = new DateTime(2026, 1, 2)
        },
        new User
        {
            Id = 3,
            FirstName = "Deleted",
            LastName = "User",
            Email = "deleted@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = true,
            CreatedDate = new DateTime(2026, 1, 3)
        }
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.TotalRecords);
        Assert.Equal(2, result.Items.Count);

        Assert.DoesNotContain(
            result.Items,
            x => x.Email == "deleted@gmail.com");
    }
    [Fact]
    public async Task GetAllUsersAsync_WithSearchTerm_ShouldFilterUsers()
    {
        // Arrange
        var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false
        },
        new User
        {
            Id = 2,
            FirstName = "Kumar",
            LastName = "Raj",
            Email = "kumar@gmail.com",
            Role = UserRole.Admin,
            IsActive = true,
            IsDeleted = false
        }
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            SearchTerm = "siva",
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync(request);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("siva@gmail.com", result.Items[0].Email);
    }
    [Fact]
    public async Task GetAllUsersAsync_WithRoleFilter_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "Subramanian",
            Email = "siva@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false
        },
        new User
        {
            Id = 2,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@gmail.com",
            Role = UserRole.Admin,
            IsActive = true,
            IsDeleted = false
        }
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            Role = UserRole.Admin,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync( request);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal(UserRole.Admin, result.Items[0].Role);
    }
    [Fact]
    public async Task GetAllUsersAsync_WithIsActiveFilter_ShouldReturnMatchingUsers()
    {
        // Arrange
        var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Active",
            LastName = "User",
            Email = "active@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false
        },
        new User
        {
            Id = 2,
            FirstName = "Inactive",
            LastName = "User",
            Email = "inactive@gmail.com",
            Role = UserRole.Student,
            IsActive = false,
            IsDeleted = false
        }
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            IsActive = false,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync(request);

        // Assert
        Assert.Single(result.Items);
        Assert.False(result.Items[0].IsActive);
    }
    [Fact]
    public async Task GetAllUsersAsync_WithCreatedFrom_ShouldReturnUsersAfterDate()
    {
        // Arrange
        var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Old",
            LastName = "User",
            Email = "old@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = new DateTime(2026, 1, 1)
        },
        new User
        {
            Id = 2,
            FirstName = "New",
            LastName = "User",
            Email = "new@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = new DateTime(2026, 2, 1)
        }
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            CreatedFrom = new DateOnly(2026, 2, 1),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync( request);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("new@gmail.com", result.Items[0].Email);
    }
    [Fact]
    public async Task GetAllUsersAsync_WithCreatedTo_ShouldIncludeEntireDate()
    {
        // Arrange
        var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Morning",
            LastName = "User",
            Email = "morning@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = new DateTime(2026, 2, 1, 10, 30, 0)
        },
        new User
        {
            Id = 2,
            FirstName = "Evening",
            LastName = "User",
            Email = "evening@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = new DateTime(2026, 2, 1, 23, 59, 59)
        },
        new User
        {
            Id = 3,
            FirstName = "Next",
            LastName = "Day",
            Email = "next@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false,
            CreatedDate = new DateTime(2026, 2, 2, 0, 0, 0)
        }
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            CreatedTo = new DateOnly(2026, 2, 1),
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync(request);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.DoesNotContain(
            result.Items,
            x => x.Email == "next@gmail.com");
    }
    [Fact]
    public async Task GetAllUsersAsync_WithSortByFirstName_ShouldReturnSortedUsers()
    {
        // Arrange
        var users = new List<User>
    {
        new User
        {
            Id = 1,
            FirstName = "Siva",
            LastName = "User",
            Email = "siva@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false
        },
        new User
        {
            Id = 2,
            FirstName = "Admin",
            LastName = "User",
            Email = "admin@gmail.com",
            Role = UserRole.Admin,
            IsActive = true,
            IsDeleted = false
        },
        new User
        {
            Id = 3,
            FirstName = "Kumar",
            LastName = "User",
            Email = "kumar@gmail.com",
            Role = UserRole.Student,
            IsActive = true,
            IsDeleted = false
        }
    };

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            SortBy = "firstname",
            SortDescending = false,
            PageNumber = 1,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync( request);

        // Assert
        Assert.Equal("Admin", result.Items[0].FirstName);
        Assert.Equal("Kumar", result.Items[1].FirstName);
        Assert.Equal("Siva", result.Items[2].FirstName);
    }
    [Fact]
    public async Task GetAllUsersAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var users = Enumerable.Range(1, 15)
            .Select(i => new User
            {
                Id = i,
                FirstName = $"User{i}",
                LastName = "Test",
                Email = $"user{i}@gmail.com",
                Role = UserRole.Student,
                IsActive = true,
                IsDeleted = false,
                CreatedDate = new DateTime(2026, 1, i)
            })
            .ToList();

        _userRepoMock
            .Setup(x => x.TableNoTracking)
            .Returns(users.BuildMock());

        var request = new UserQueryParameters
        {
            PageNumber = 2,
            PageSize = 10
        };

        // Act
        var result = await _authService.GetAllUsersAsync(request);

        // Assert
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(10, result.PageSize);

        Assert.Equal(15, result.TotalRecords);
        Assert.Equal(2, result.TotalPages);

        Assert.Equal(5, result.Items.Count);

        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }
}

