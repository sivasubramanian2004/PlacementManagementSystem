using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Core.DTOs.Departments;
using PMS.Core.DTOs.Students;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using PMS.Data.UnitOfWork;
using PMS.Service.FileStorage;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;

namespace PMS.Service.Students
{
    public class StudentService : IStudentService
    {
        private readonly ILogger<StudentService> _logger;
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<EducationDetails> _educationRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUrlHelperService _urlHelper;
        private readonly IFileStorageService _fileStorageService;

        public StudentService(ILogger<StudentService> logger, IRepository<Student> studentRepo, IRepository<Department> departmentRepo,
            IRepository<EducationDetails> educationRepo, IRepository<User> userRepo, IUnitOfWork unitOfWork, IUrlHelperService urlHelper,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _studentRepo = studentRepo;
            _departmentRepo = departmentRepo;
            _educationRepo = educationRepo;
            _userRepo = userRepo;
            _unitOfWork = unitOfWork;
            _urlHelper = urlHelper;
            _fileStorageService = fileStorageService;
        }

        public async Task<StudentResponseDto> CreateStudentAsync(CreateStudentRequestDto dto, int createdBy)
        {
            // 1. Normalize input
            var email = dto.Email.Trim();
            var registerNumber =
                dto.RegisterNumber.Trim().ToUpperInvariant();

            // 2. Find ACTIVE User
            var user = await _userRepo.TableNoTracking
                .FirstOrDefaultAsync(u =>
                    u.Email == email &&
                    !u.IsDeleted &&
                    u.IsActive);

            if (user == null)
            {
                throw new KeyNotFoundException(
                    $"No active registered account found for email '{email}'.");
            }

            // 3. Check ACTIVE Student profile
            var studentExists = await _studentRepo.TableNoTracking
                .AnyAsync(s =>
                    !s.IsDeleted &&
                    s.IsActive &&
                    (s.UserId == user.Id ||
                     s.RegisterNumber == registerNumber));

            if (studentExists)
            {
                throw new InvalidOperationException(
                    "An active student profile already exists for this account " +
                    "or register number.");
            }

            // 4. Check Department exists and is active
            var departmentExists = await _departmentRepo.TableNoTracking
                .AnyAsync(d =>
                    d.Id == dto.DepartmentId &&
                    !d.IsDeleted &&
                    d.IsActive);

            if (!departmentExists)
            {
                throw new KeyNotFoundException(
                    $"Active Department with Id {dto.DepartmentId} not found.");
            }
            // 5. Upload files
            FileUploadResult? resumePath = null;
            FileUploadResult? photoPath = null;

            var photoFolder = Path.Combine("StudentDocuments", "StudentPhotos");
            var resumeFolder = Path.Combine("StudentDocuments", "StudentResumes");
            try
            {
                // Profile photo is optional
                if (dto.ProfilePhoto != null)
                {
                    photoPath = await _fileStorageService.UploadAsync(dto.ProfilePhoto, photoFolder);

                }
                // Resume is required
                resumePath = await _fileStorageService.UploadAsync(dto.Resume, resumeFolder);

                // 6. Create Student
                var student = new Student
                {
                    UserId = user.Id,

                    RegisterNumber = registerNumber,

                    DepartmentId = dto.DepartmentId,

                    Gender = dto.Gender,

                    Phone = dto.Phone?.Trim(),

                    Skills = dto.Skills.Trim(),

                    PlacementStatus = dto.PlacementStatus,

                    ResumeUrl = resumePath.RelativePath,

                    ProfilePictureUrl = photoPath?.RelativePath,

                    CreatedBy = createdBy

                };
                // 7. Add Student
                await _studentRepo.AddAsync(student);
                // 8. Add Education Details
                foreach (var educationDto in dto.Educations)
                {
                    student.Educations.Add(new EducationDetails
                    {
                        EducationType = educationDto.EducationType,

                        Institution = educationDto.Institution.Trim(),

                        PercentageOrCgpa = educationDto.PercentageOrCgpa,

                        Backlogs = educationDto.Backlogs,

                        YearOfPassing = educationDto.YearOfPassing,

                        Location = educationDto.Location?.Trim(),

                        CreatedBy = createdBy
                    });
                }
                // 9. Save Student + Education
                await _unitOfWork.SaveChangesAsync();
                // 10. Logging
                _logger.LogInformation("Student created successfully for email {Email}", email);

                // 11. Return Response
                return new StudentResponseDto
                {
                    StudentId = student.Id,

                    Email = user.Email,

                    RegisterNumber = student.RegisterNumber,

                    Gender = student.Gender,

                    PlacementStatus = student.PlacementStatus,

                    ResumeUrl = _urlHelper.BuildFullUrl(
               student.ResumeUrl) ?? string.Empty
                };
            }
            catch
            { // 12. Cleanup uploaded files if DB operation fails
              // --------------------------------------------------

                if (resumePath != null)
                {
                    await _fileStorageService.DeleteAsync(
                        resumePath.FullPath);
                }

                if (photoPath != null)
                {
                    await _fileStorageService.DeleteAsync(
                        photoPath.FullPath);
                }

                throw;
            }
        }

        public async Task DeleteStudentAsync(int id, int deletedBy)
        {
            // 1. Get Student + Education records
            var student = await _studentRepo.Table
                         .Include(s => s.Educations)
                         .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (student == null)
            {
                throw new KeyNotFoundException($"Student with Id {id} not found.");

            }
            // 2. Store document paths before soft delete
            var oldResumeUrl = student.ResumeUrl;
            var oldProfilePictureUrl = student.ProfilePictureUrl;
            var deletedDate = DateTime.UtcNow;

            // =====================================================
            // 3. Soft delete Student
            // =====================================================
            student.IsDeleted = true;
            student.IsActive= false;
            student.DeletedDate = deletedDate;
            student.DeletedBy = deletedBy;
            // =====================================================
            // 4. Soft delete Education records
            // =====================================================

            var educationsToDelete = student.Educations
                                    .Where(e => !e.IsDeleted).ToList();

            foreach (var education in educationsToDelete)
            {
                education.IsDeleted = true;
                education.IsActive = false;
                education.DeletedDate = deletedDate;
                education.DeletedBy = deletedBy;
            }
            // =====================================================
            // 5. Save DB
            // =====================================================

            await _unitOfWork.SaveChangesAsync();

            // =====================================================
            // 6. DB SUCCESS
            //    Now delete physical documents
            // =====================================================

            try
            {
                // Delete Resume
                if (!string.IsNullOrWhiteSpace(oldResumeUrl))
                {
                    var resumeFullPath = await _fileStorageService.GetFullPath(oldResumeUrl);

                    await _fileStorageService.DeleteAsync(resumeFullPath);

                }

                // Delete Profile Photo
                if (!string.IsNullOrWhiteSpace(oldProfilePictureUrl))
                {
                    var photoFullPath = await _fileStorageService.GetFullPath(oldProfilePictureUrl);

                    await _fileStorageService.DeleteAsync(photoFullPath);
                }
            }
            catch (Exception ex)
            {
                // DB is already successfully soft-deleted.
                // Do not throw and undo the DB operation.
                // Log the file deletion failure for cleanup later.

                _logger.LogError(ex, "Student soft-deleted successfully, but document deletion failed. " + "StudentId: {StudentId}",
                    id);
            }

            // =====================================================
            // 7. Logging
            // =====================================================

            _logger.LogInformation(
                "Student soft-deleted successfully. " +
                "StudentId: {StudentId}, " +
                "EducationRecordsDeleted: {EducationCount}, " +
                "DeletedBy: {DeletedBy}", id, educationsToDelete.Count, deletedBy);

        }

        public async Task<PagedResult<StudentResponseDto>> GetAllStudentAsync(StudentQueryParameters request)
        {

            var student = _studentRepo.TableNoTracking
                          .Where(s => s.IsDeleted != true);

            if (student == null)
            {
                throw new KeyNotFoundException("Profile not found");
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();

                student = student.Where(s =>
                       s.User.FirstName.Contains(searchTerm)
                    || s.User.LastName.Contains(searchTerm)
                    || s.User.Email.Contains(searchTerm)
                    || s.RegisterNumber.Contains(searchTerm));
            }
            if (request.DepartmentId.HasValue)
            {
                student = student.Where(s => s.DepartmentId == request.DepartmentId.Value);
            }
            if (request.IsActive.HasValue)
            {
                student = student.Where(s => s.IsActive == request.IsActive);
            }
            if (request.Role.HasValue)
            {
                student = student.Where(s => s.User.Role == request.Role);
            }
            if (request.Gender.HasValue)
            {
                student = student.Where(s => s.Gender == request.Gender);
            }
            if (request.PlacementStatus.HasValue)
            {
                student = student.Where(s => s.PlacementStatus == request.PlacementStatus);
            }
            //Created Date From
            if (request.CreatedFrom.HasValue)
            {
                var fromDate = request.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue);
                student = student.Where(c => c.CreatedDate >= fromDate);

            }

            // Created Date To
            if (request.CreatedTo.HasValue)
            {
                var toDate = request.CreatedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);

                student = student.Where(c =>
                    c.CreatedDate < toDate);
            }

            var sortOptions = new Dictionary<string, Expression<Func<Student, object?>>>
            {
                ["RegisterNumber"] = s => s.RegisterNumber,
                ["FirstName"] = s => s.User.FirstName,
                ["LastName"] = s => s.User.LastName,
                ["email"] = s => s.User.Email,
                ["role"] = s => s.User.Role,
                ["createddate"] = s => s.CreatedDate
            };
            student = student.ApplySorting(request, sortOptions, defaultSort: e => (object?)e.Id);

            var resultquery = student.Select(s => new StudentResponseDto
            {
                StudentId = s.Id,
                FirstName = s.User.FirstName,
                LastName = s.User.LastName,
                Email = s.User.Email,
                RegisterNumber = s.RegisterNumber,
                DepartmentName = s.Department.Name,
                Role = s.User.Role,
                PlacementStatus = s.PlacementStatus,
                Gender = s.Gender,
                ResumeUrl = _urlHelper.BuildFullUrl(s.ResumeUrl) ?? string.Empty,
            });
            return await resultquery.ToPagedResultAsync(request);
        }

        public async Task<StudentBasicDto> GetStudentByIdAsync(int id)
        {
            var result = await _studentRepo.TableNoTracking
                .Where(s => s.Id == id && !s.IsDeleted)
                .Select(s => new StudentBasicDto
                {
                    StudentId = s.Id,

                    UserId = s.UserId,

                    FirstName = s.User.FirstName,

                    LastName = s.User.LastName,

                    Email = s.User.Email,

                    RegisterNumber = s.RegisterNumber,

                    DepartmentName = s.Department.Name,

                    Role = s.User.Role,

                    PlacementStatus = s.PlacementStatus,

                    Gender = s.Gender,

                    Phone = s.Phone,

                    Status = s.IsActive ? "Active" : "Inactive",

                    ProfilePictureUrl = s.ProfilePictureUrl,

                    ResumeUrl = s.ResumeUrl,

                    Educations = s.Educations
                                .Where(e => !e.IsDeleted)
                                .Select(e => new EducationDto
                                {
                                    EducationId = e.Id,
                                    EducationType = e.EducationType,
                                    Institution = e.Institution,
                                    PercentageOrCgpa = e.PercentageOrCgpa,
                                    Backlogs = e.Backlogs,
                                    YearOfPassing = e.YearOfPassing,
                                    Location = e.Location
                                })
                                 .ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                throw new KeyNotFoundException($"Student with Id {id} not found.");
            }

            result.ProfilePictureUrl = _urlHelper.BuildFullUrl(result.ProfilePictureUrl) ?? string.Empty;

            result.ResumeUrl = _urlHelper.BuildFullUrl(result.ResumeUrl) ?? string.Empty;

            return result;
        }
        public async Task<int> GetStudentId(int userId)
        {
            var studentId = await _studentRepo.TableNoTracking
                            .Where(s => s.UserId == userId && !s.IsDeleted)
                            .Select(s => (int?)s.Id)
                            .FirstOrDefaultAsync();

            if (!studentId.HasValue)
                throw new KeyNotFoundException(
                    "Student profile not found for the current user.");

            return studentId.Value;
        }
        public async Task<StudentBasicDto> GetMyProfileAsync(int StudentId)
        {
            var result = await _studentRepo.TableNoTracking
                .Where(s => s.Id == StudentId && !s.IsDeleted)
                .Select(s => new StudentBasicDto
                {
                    StudentId = s.Id,
                    UserId = s.UserId,
                    FirstName = s.User.FirstName,
                    LastName = s.User.LastName,
                    Email = s.User.Email,
                    RegisterNumber = s.RegisterNumber,
                    DepartmentName = s.Department.Name,
                    Role = s.User.Role,
                    PlacementStatus = s.PlacementStatus,
                    Gender = s.Gender,
                    Phone = s.Phone,
                    Status = s.IsActive ? "Active" : "Inactive",
                    ProfilePictureUrl = _urlHelper.BuildFullUrl(s.ProfilePictureUrl) ?? string.Empty,
                    ResumeUrl = _urlHelper.BuildFullUrl(s.ResumeUrl) ?? string.Empty,
                    Educations = s.Educations
                                .Where(e => !e.IsDeleted)
                                .Select(e => new EducationDto
                                {
                                    EducationId = e.Id,
                                    EducationType = e.EducationType,
                                    Institution = e.Institution,
                                    PercentageOrCgpa = e.PercentageOrCgpa,
                                    Backlogs = e.Backlogs,
                                    YearOfPassing = e.YearOfPassing,
                                    Location = e.Location
                                })
                                 .ToList()
                })
                .FirstOrDefaultAsync();

            if (result == null)
            {
                throw new KeyNotFoundException($"Student with Id {StudentId} not found.");
            }

            return result;
        }
        public async Task UpdateStudentAsync(int id, UpdateStudentRequestDto dto, int updatedBy)
        {
            // =========================================================
            // 1. Get existing student
            // =========================================================

            var student = await _studentRepo.Table
                         .Include(s => s.User)
                         .Include(s => s.Educations)
                         .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (student == null)
            {
                throw new KeyNotFoundException($"Student with Id {id} not found.");

            }
            // =========================================================
            // 2. Store old file paths
            // =========================================================

            var oldResumeUrl = student.ResumeUrl;
            var oldProfilePictureUrl = student.ProfilePictureUrl;

            // =========================================================
            // 3. Variables for new uploaded files
            // =========================================================
            FileUploadResult? newResume = null;
            FileUploadResult? newPhoto = null;

            var resumeFolder = Path.Combine("StudentDocuments", "StudentResumes");
            var photoFolder = Path.Combine("StudentDocuments", "StudentPhotos");

            try
            {
                // =====================================================
                // 4. Department validation
                // =====================================================

                if (dto.DepartmentId.HasValue)
                {
                    var departmentExists = await _departmentRepo.TableNoTracking
                                          .AnyAsync(d => d.Id == dto.DepartmentId.Value && !d.IsDeleted);

                    if (!departmentExists)
                    {
                        throw new KeyNotFoundException($"Department with Id " +
                                        $"{dto.DepartmentId.Value} not found.");
                    }
                }
                // =====================================================
                // 5. Register Number validation
                // =====================================================

                string? registerNumber = null;

                if (!string.IsNullOrWhiteSpace(dto.RegisterNumber))
                {
                    registerNumber = dto.RegisterNumber.Trim().ToUpperInvariant();

                    var registerNumberExists = await _studentRepo.TableNoTracking
                                              .AnyAsync(s => s.Id != id && !s.IsDeleted && s.RegisterNumber == registerNumber);

                    if (registerNumberExists)
                    {
                        throw new InvalidOperationException(
                            "Student with the same register number already exists.");
                    }
                }
                // =====================================================
                // 6. Upload NEW Resume if provided
                // =====================================================

                if (dto.Resume != null)
                {
                    newResume = await _fileStorageService.UploadAsync(dto.Resume, resumeFolder);

                }
                // =====================================================
                // 7. Upload NEW Profile Photo if provided
                // =====================================================

                if (dto.ProfilePhoto != null)
                {
                    newPhoto = await _fileStorageService.UploadAsync(dto.ProfilePhoto, photoFolder);

                }

                // =====================================================
                // 8. Update User fields only if provided
                // =====================================================

                if (!string.IsNullOrWhiteSpace(dto.FirstName))
                {
                    student.User.FirstName = dto.FirstName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.LastName))
                {
                    student.User.LastName = dto.LastName.Trim();

                }
                student.User.UpdatedBy = updatedBy;
                student.User.UpdatedDate = DateTime.UtcNow;
                // =====================================================
                // 9. Update Student fields only if provided
                // =====================================================
                if (registerNumber != null)
                {
                  
                    student.RegisterNumber = registerNumber;
                }

                if (dto.DepartmentId.HasValue)
                {
                    student.DepartmentId = dto.DepartmentId.Value;
                }

                if (dto.Gender.HasValue)
                {
                    student.Gender = dto.Gender.Value;
                }

                if (dto.Phone != null)
                {
                    student.Phone = dto.Phone.Trim();
                }

                if (dto.Skills != null)
                {
                    student.Skills = dto.Skills.Trim();
                }

                if (dto.PlacementStatus.HasValue)
                {
                    student.PlacementStatus =dto.PlacementStatus.Value;
                }
                if (dto.IsActive.HasValue)
                {
                    student.IsActive =
                        dto.IsActive.Value;
                }
                // =====================================================
                // 10. Replace Resume only when new Resume provided
                // =====================================================

                if (newResume != null)
                {
                    student.ResumeUrl = newResume.RelativePath;

                }
                // =====================================================
                // 11. Replace Profile Photo only when new photo provided
                // =====================================================
                if (newPhoto != null)
                {
                    student.ProfilePictureUrl = newPhoto.RelativePath;

                }
                // =====================================================
                // 12. Update Student audit fields
                // =====================================================
                student.UpdatedBy = updatedBy;
                student.UpdatedDate = DateTime.UtcNow;

                // =====================================================
                // 13. Update / Add Education
                // =====================================================

                if (dto.Educations != null)
                {
                    foreach (var educationDto in dto.Educations)
                    {
                        // -------------------------------------------------
                        // Add NEW education
                        // -------------------------------------------------

                        if (educationDto.EducationId == 0)
                        {
                            student.Educations.Add(
                                new EducationDetails
                                {
                                    EducationType = educationDto.EducationType,

                                    Institution = educationDto.Institution.Trim(),

                                    PercentageOrCgpa = educationDto.PercentageOrCgpa,

                                    Backlogs = educationDto.Backlogs,

                                    YearOfPassing = educationDto.YearOfPassing,

                                    Location = educationDto.Location?.Trim(),

                                    CreatedBy = updatedBy,

                                    CreatedDate = DateTime.UtcNow
                                });

                            continue;
                        }
                        // -------------------------------------------------
                        // Update EXISTING education
                        // -------------------------------------------------

                        var education = student.Educations
                                       .FirstOrDefault(e => e.Id == educationDto.EducationId && !e.IsDeleted);

                        if (education == null)
                        {
                            throw new KeyNotFoundException($"Education with Id " +
                                $"{educationDto.EducationId} not found.");
                        }

                        education.EducationType = educationDto.EducationType;

                        education.Institution = educationDto.Institution.Trim();

                        education.PercentageOrCgpa = educationDto.PercentageOrCgpa;

                        education.Backlogs = educationDto.Backlogs;

                        education.YearOfPassing = educationDto.YearOfPassing;

                        education.Location = educationDto.Location?.Trim();

                        education.UpdatedBy = updatedBy;

                        education.UpdatedDate = DateTime.UtcNow;
                    }
                }
                // =====================================================
                // 14. SAVE DATABASE
                // =====================================================

                await _studentRepo.UpdateAsync(student);

                // =====================================================
                // 15. DB SUCCESS
                //     Now delete OLD Resume
                // =====================================================

                if (newResume != null && !string.IsNullOrWhiteSpace(oldResumeUrl))

                {
                    var oldResumeFullPath = await _fileStorageService.GetFullPath(oldResumeUrl);

                    await _fileStorageService.DeleteAsync(oldResumeFullPath);

                }
                // =====================================================
                // 16. DB SUCCESS
                //     Now delete OLD Profile Photo
                // =====================================================

                if (newPhoto != null && !string.IsNullOrWhiteSpace(oldProfilePictureUrl))
                {

                    var oldPhotoFullPath = await _fileStorageService.GetFullPath(oldProfilePictureUrl);

                    await _fileStorageService.DeleteAsync( oldPhotoFullPath);
                       
                }

                // =====================================================
                // 17. Logging
                // =====================================================

                _logger.LogInformation("Student updated successfully. " +
                    "StudentId: {StudentId}, UpdatedBy: {UpdatedBy}", id, updatedBy);
            }
            catch
            {
                // =====================================================
                // 18. DB/upload failed
                //     Delete NEW files
                // =====================================================

                if (newResume != null)
                {
                    await _fileStorageService.DeleteAsync(newResume.FullPath);
                }

                if (newPhoto != null)
                {
                    await _fileStorageService.DeleteAsync(newPhoto.FullPath);
                }
                throw;
            }
        }

    }
}