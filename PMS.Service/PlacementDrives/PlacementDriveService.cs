using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Core.DTOs.PlacementDrives;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using PMS.Data.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
namespace PMS.Service.PlacementDrives
{
    public class PlacementDriveService : IPlacementDriveService
    {
        private readonly IRepository<PlacementDrive> _placementDriveRepo;

        private readonly IRepository<Company> _companyRepo;

        private readonly IRepository<Department> _departmentRepo;

        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<PlacementDrive> _logger;

        public PlacementDriveService(IRepository<PlacementDrive> placementDriveRepo, ILogger<PlacementDrive> logger,
            IRepository<Company> companyRepo, IRepository<Department> departmentRepo, IUnitOfWork unitOfWork)
        {

            _placementDriveRepo = placementDriveRepo;

            _logger = logger;

            _companyRepo = companyRepo;

            _departmentRepo = departmentRepo;

            _unitOfWork = unitOfWork;


        }

        public async Task<PlacementDriveResponseDto> CreateAsync(CreatePlacementDriveRequestDto dto, int createdBy)
        {
            // Check Company
            var companyExists = await _companyRepo.TableNoTracking
                               .AnyAsync(c => c.Id == dto.CompanyId &
                               !c.IsDeleted && c.IsActive);

            if (!companyExists)
                throw new KeyNotFoundException($"Company with id {dto.CompanyId} not found or inactive.");


            // Remove duplicate department IDs
            var departmentIds = dto.DepartmentIds.Distinct().ToList();

            // Check Departments
            var validDepartmentIds = await _departmentRepo.TableNoTracking
                .Where(d => departmentIds.Contains(d.Id) && !d.IsDeleted && d.IsActive)
                .Select(d => d.Id)
                .ToListAsync();

            if (validDepartmentIds.Count != departmentIds.Count)
            {
                throw new KeyNotFoundException(
                    "One or more department IDs are invalid or inactive.");
            }

            var placementDrive = new PlacementDrive
            {
                CompanyId = dto.CompanyId,
                JobTitle = dto.JobTitle.Trim(),
                JobDescription = dto.JobDescription.Trim(),
                EmploymentType = dto.EmploymentType,
                WorkMode = dto.WorkMode,
                Location = dto.Location.Trim(),
                MinimumCgpa = dto.MinimumCgpa,
                MaximumBacklogs = dto.MaximumBacklogs,
                GraduationYear = dto.GraduationYear,
                Salary = dto.Salary,
                RequiredSkills = dto.RequiredSkills?.Trim(),
                ApplicationDeadline = dto.ApplicationDeadline,
                DriveDate = dto.DriveDate,
                Status = PlacementDriveStatus.Draft,
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow
            };

            foreach (var departmentId in departmentIds)
            {
                placementDrive.PlacementDriveDepartments.Add(
                    new PlacementDriveDepartment
                    {
                        DepartmentId = departmentId,
                        CreatedBy = createdBy,
                        CreatedDate = DateTime.UtcNow
                    });
            }

            await _placementDriveRepo.AddAsync(placementDrive);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Placement drive {PlacementDriveId} created successfully by {CreatedBy}.", placementDrive.Id,
                createdBy);

            return new PlacementDriveResponseDto
            {
                PlacementDriveId = placementDrive.Id,
                CompanyName = placementDrive.Company.Name,
                JobTitle = placementDrive.JobTitle,
                JobDescription = placementDrive.JobDescription,
                EmploymentType = placementDrive.EmploymentType,
                WorkMode = placementDrive.WorkMode,
                ApplicationDeadline = DateOnly.FromDateTime(placementDrive.ApplicationDeadline),
                DriveStatus = placementDrive.Status,
                DriveDate = placementDrive.DriveDate.HasValue ? DateOnly.FromDateTime(placementDrive.DriveDate.Value) : null
            };
        }

        public async Task UpdateAsync(int id, UpdatePlacementDriveRequestDto dto, int updatedBy)
        {
            var placementDrive = await _placementDriveRepo.Table
                                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (placementDrive == null)
                throw new KeyNotFoundException(
                    $"Placement drive with id {id} not found.");

            // Company
            if (dto.CompanyId.HasValue)
            {
                var companyExists = await _companyRepo.TableNoTracking
                    .AnyAsync(c => c.Id == dto.CompanyId.Value && !c.IsDeleted && c.IsActive);

                if (!companyExists)
                    throw new KeyNotFoundException(
                        $"Company with id {dto.CompanyId.Value} not found or inactive.");

                placementDrive.CompanyId = dto.CompanyId.Value;
            }

            // Job title
            if (dto.JobTitle != null)
            {
                if (string.IsNullOrWhiteSpace(dto.JobTitle))
                    throw new ArgumentException("Job title cannot be empty.");

                placementDrive.JobTitle = dto.JobTitle.Trim();
            }

            // Job description
            if (dto.JobDescription != null)
            {
                if (string.IsNullOrWhiteSpace(dto.JobDescription))
                    throw new ArgumentException(
                        "Job description cannot be empty.");

                placementDrive.JobDescription = dto.JobDescription.Trim();
            }

            // Employment type
            if (dto.EmploymentType.HasValue)
                placementDrive.EmploymentType = dto.EmploymentType.Value;

            // Work mode
            if (dto.WorkMode.HasValue)
                placementDrive.WorkMode = dto.WorkMode.Value;

            // Location
            if (dto.Location != null)
            {
                if (string.IsNullOrWhiteSpace(dto.Location))
                    throw new ArgumentException(
                        "Location cannot be empty.");

                placementDrive.Location = dto.Location.Trim();
            }

            // CGPA
            if (dto.MinimumCgpa.HasValue)
                placementDrive.MinimumCgpa = dto.MinimumCgpa.Value;

            // Backlogs
            if (dto.MaximumBacklogs.HasValue)
                placementDrive.MaximumBacklogs = dto.MaximumBacklogs.Value;

            // Graduation year
            if (dto.GraduationYear.HasValue)
                placementDrive.GraduationYear = dto.GraduationYear.Value;

            // Salary
            if (dto.Salary.HasValue)
                placementDrive.Salary = dto.Salary.Value;

            // Required skills
            if (dto.RequiredSkills != null)
            {
                placementDrive.RequiredSkills =
                    string.IsNullOrWhiteSpace(dto.RequiredSkills)
                        ? null
                        : dto.RequiredSkills.Trim();
            }

            // Application deadline
            if (dto.ApplicationDeadline.HasValue)
                placementDrive.ApplicationDeadline =
                    dto.ApplicationDeadline.Value;

            // Drive date
            if (dto.DriveDate.HasValue)
                placementDrive.DriveDate = dto.DriveDate.Value;

            // Status
            if (dto.Status.HasValue)
                placementDrive.Status = dto.Status.Value;

            // Departments
            if (dto.DepartmentIds != null)
            {
                var departmentIds = dto.DepartmentIds
                    .Distinct()
                    .ToList();

                var validDepartmentIds = await _departmentRepo.TableNoTracking
                    .Where(d =>
                        departmentIds.Contains(d.Id) &&
                        !d.IsDeleted &&
                        d.IsActive)
                    .Select(d => d.Id)
                    .ToListAsync();

                if (validDepartmentIds.Count != departmentIds.Count)
                {
                    throw new KeyNotFoundException(
                        "One or more department IDs are invalid or inactive.");
                }

                // Soft delete existing relationships
                foreach (var existing in placementDrive.PlacementDriveDepartments
                             .Where(x => !x.IsDeleted))
                {
                    existing.IsDeleted = true;
                    existing.DeletedBy = updatedBy;
                    existing.DeletedDate = DateTime.UtcNow;
                }

                // Add new relationships
                foreach (var departmentId in departmentIds)
                {
                    placementDrive.PlacementDriveDepartments.Add(
                        new PlacementDriveDepartment
                        {
                            DepartmentId = departmentId,
                            CreatedBy = updatedBy,
                            CreatedDate = DateTime.UtcNow
                        });
                }
            }

            // Audit
            placementDrive.UpdatedBy = updatedBy;
            placementDrive.UpdatedDate = DateTime.UtcNow;

            // Already tracked → no UpdateAsync required
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Placement drive {PlacementDriveId} updated by {UpdatedBy}.", id, updatedBy);

        }

        public async Task DeleteAsync(int id, int deletedBy) { 
         
            var Drive=await _placementDriveRepo.Table.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            if (Drive == null) {
                throw new KeyNotFoundException("Placement drive not found.");
            }

            Drive.IsDeleted = true;
            Drive.IsActive = false;
            Drive.DeletedBy = deletedBy;
            Drive.DeletedDate = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Placement drive {PlacementDriveId} deleted by {DeletedBy}.", id, deletedBy);
        }

        public async Task<PlacementDriveDto> GetPlacementDriveByIdAsync(int id)
        {
            var placementDrive =await _placementDriveRepo.TableNoTracking
                                .Where(p => p.Id == id && !p.IsDeleted)
                                .Select(p => new PlacementDriveDto
                                {

                                    PlacementDriveId = p.Id,
                                    CompanyName = p.Company.Name,
                                    JobTitle = p.JobTitle,
                                    JobDescription = p.JobDescription,
                                    EmploymentType = p.EmploymentType,
                                    WorkMode = p.WorkMode,
                                    Location = p.Location,
                                    MinimumCgpa = p.MinimumCgpa,
                                    MaximumBacklogs = p.MaximumBacklogs,
                                    GraduationYear = p.GraduationYear,
                                    Salary = p.Salary,
                                    RequiredSkills = p.RequiredSkills,
                                    ApplicationDeadline = DateOnly.FromDateTime(p.ApplicationDeadline),
                                    DriveDate = p.DriveDate.HasValue ? DateOnly.FromDateTime(p.DriveDate.Value) : null,
                                    DriveStatus = p.Status,
                                    DepartmentNames = p.PlacementDriveDepartments
                                                     .Where(pd => !pd.IsDeleted)
                                                     .Select(pd => pd.Department.Name)
                                                     .ToList()
                                })
                                .FirstOrDefaultAsync();

            if (placementDrive == null)
            {
                throw new KeyNotFoundException("Placement drive not found.");
            }

            return placementDrive;
           
        }

        public async Task<PagedResult<PlacementDriveResponseDto>> GetAllPlacementDriveAsync(PlacementDriveQueryParameters request)
        {

                var query =  _placementDriveRepo.TableNoTracking
                            .Where(p =>!p.IsDeleted);

            if (query == null)
            {
                throw new KeyNotFoundException("No departments found.");
            }
            //Global Searching across multiple fields such as firstname, lastname, email, role
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();

                query = query.Where(d =>
                    d.Company.Name.Contains(searchTerm) ||
                    d.JobTitle.Contains(searchTerm) ||
                    d.JobDescription.Contains(searchTerm) ||
                    (d.RequiredSkills != null && d.RequiredSkills.Contains(searchTerm)) ||
                    d.PlacementDriveDepartments.Any(pd => pd.Department.Name.Contains(searchTerm)) ||
                    d.Location.Contains(searchTerm));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(d => d.IsActive == request.IsActive.Value);
            }
            if (request.EmploymentType.HasValue)
            {
                query = query.Where(d => d.EmploymentType == request.EmploymentType.Value);
            }
            if (request.WorkMode.HasValue)
            {
                query = query.Where(d => d.WorkMode == request.WorkMode.Value);
            }
            if (request.DriveStatus.HasValue)
            {
                query = query.Where(d => d.Status== request.DriveStatus.Value);
            }
            if (request.GraduationYear.HasValue)
            {
                query = query.Where(d => d.GraduationYear == request.GraduationYear.Value);

            }
            if (request.DepartmentId.HasValue)
            {
                query = query.Where(d => d.PlacementDriveDepartments.Any(pd => pd.DepartmentId == request.DepartmentId.Value));
            }
            if (request.CompanyId.HasValue)
            {
                query = query.Where(d => d.CompanyId == request.CompanyId.Value);
            }
            if (request.DriveDate.HasValue)
            {
                var driveDate = request.DriveDate.Value.ToDateTime(TimeOnly.MinValue);
                var nextDate = driveDate.AddDays(1);

                query = query.Where(d =>
                    d.DriveDate >= driveDate &&
                    d.DriveDate < nextDate);
            }


            var sortOptions = new Dictionary<string, Expression<Func<PlacementDrive, object?>>>
            {
                ["MinimumCgpa"] = d => d.MinimumCgpa,
                ["MaximumBacklogs"] = d => d.MaximumBacklogs,
                ["GraduationYear"] = d => d.GraduationYear,
                ["Salary"] = d => d.Salary,
                ["ApplicationDeadline"] = d => d.ApplicationDeadline,
                ["DriveDate"] = d => d.DriveDate

            };

            query = query.ApplySorting(request, sortOptions, defaultSort: e => (object?)e.Id);

            var resultQuery= query.Select(p => new PlacementDriveResponseDto
                                 {
                                     PlacementDriveId = p.Id,
                                     CompanyName = p.Company.Name,
                                     JobTitle = p.JobTitle,
                                     JobDescription = p.JobDescription,
                                     EmploymentType = p.EmploymentType,
                                     WorkMode = p.WorkMode,
                                     ApplicationDeadline = DateOnly.FromDateTime(p.ApplicationDeadline),
                                     DriveStatus = p.Status,
                                     DriveDate = p.DriveDate.HasValue
                                               ? DateOnly.FromDateTime(p.DriveDate.Value) :null//ternary operation,


            });
                   
            return await resultQuery.ToPagedResultAsync(request);


        }
    }  
}


