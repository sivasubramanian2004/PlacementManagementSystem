using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Core.DTOs.Students;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using PMS.Data.UnitOfWork;
using PMS.Service.FileStorage;
using System;
using System.Collections.Generic;
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
            var registerNumber = dto.RegisterNumber.Trim().ToUpperInvariant();
            // 2. Find User
            var user = await _userRepo.TableNoTracking
                      .FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted);

            if (user == null)
            {
                throw new KeyNotFoundException(
                    $"No registered account found for email '{email}'.");
            }
            // 3. Check whether Student already exists
            var studentExists = await _studentRepo.TableNoTracking
                                .AnyAsync(s => !s.IsDeleted && (s.UserId == user.Id || s.RegisterNumber == registerNumber));

            if (studentExists)
            {
                throw new InvalidOperationException(
                    "A student profile already exists for this account " +
                    "or register number.");
            }
            // 4. Check Department exists
            var departmentExists = await _departmentRepo.TableNoTracking
                                  .AnyAsync(d => d.Id == dto.DepartmentId && !d.IsDeleted);
            if (!departmentExists)
            {
                throw new KeyNotFoundException(
                    $"Department with Id {dto.DepartmentId} not found.");
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

                    Name = dto.Name,

                    Gender = dto.Gender,

                    Phone = dto.Phone?.Trim(),

                    Skills = dto.Skills.Trim(),

                    PlacementStatus = dto.PlacementStatus,

                    ResumeUrl = resumePath.RelativePath,

                    ProfilePictureUrl = photoPath?.RelativePath,

                    CreatedBy=createdBy
                    
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

                    Name = student.Name,

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

    }
}
