using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Core.DTOs.Applications;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using PMS.Data.UnitOfWork;
using PMS.Service.Email;
using PMS.Service.Students;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
namespace PMS.Service.Applicationss
{
    public class ApplicationService : IApplicationService
    {
        private readonly IRepository<ApplicationNumberCounter> _applicationNumberCounterRepo;
        private readonly IRepository<Application> _applicationRepo;

        private readonly IRepository<Student> _studentRepo;

        private readonly IRepository<EducationDetails> _educationRepo;

        private readonly IRepository<PlacementDrive> _placementDriveRepo;

        private readonly IRepository<Company> _companyRepo;

        private readonly IRepository<Department> _departmentRepo;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IEmailService _emailService;

        private readonly ILogger<PlacementDrive> _logger;

        public ApplicationService(IRepository<ApplicationNumberCounter> applicationNumberCounterRepo,IRepository<Application> applicationRepo, IRepository<Student> studentRepo, IRepository<EducationDetails> educationRepo, IRepository<PlacementDrive> placementDriveRepo, ILogger<PlacementDrive> logger,
            IRepository<Company> companyRepo, IRepository<Department> departmentRepo, IUnitOfWork unitOfWork, IEmailService emailService)
        {

            _applicationNumberCounterRepo = applicationNumberCounterRepo;

            _applicationRepo = applicationRepo;

            _studentRepo = studentRepo;

            _educationRepo = educationRepo;

            _placementDriveRepo = placementDriveRepo;

            _logger = logger;

            _companyRepo = companyRepo;

            _departmentRepo = departmentRepo;

            _unitOfWork = unitOfWork;

            _emailService = emailService;


        }
        private async Task<string> GenerateApplicationNumberAsync(int year)
        {
            var counter = await _applicationNumberCounterRepo.Table
                .FirstOrDefaultAsync(x => x.Year == year);

            if (counter == null)
            {
                counter = new ApplicationNumberCounter
                {
                    Year = year,
                    LastSequence = 1
                };

                await _applicationNumberCounterRepo.AddAsync(counter);
            }
            else
            {
                counter.LastSequence++;
            }

            return $"A{year % 100:00}{counter.LastSequence:D4}";
        }
        public async Task<ApplicationResponseDto> CreateAsync(CreateApplicationRequestDto dto, int UserId)
        {
            // 1. Find Student profile for logged-in user

            var student = await _studentRepo.TableNoTracking
                .Include(s => s.User)
                .Include(s=>s.Department)
                .FirstOrDefaultAsync(s =>
                    s.UserId == UserId &&
                    !s.IsDeleted);

            if (student == null)
                throw new KeyNotFoundException(
                    "Student profile not found.");

            // 2. Get Placement Drive
            var placementDrive = await _placementDriveRepo.TableNoTracking
                .Include(p => p.Company)
                .Include(p => p.PlacementDriveDepartments)
                .FirstOrDefaultAsync(p =>
                    p.Id == dto.PlacementDriveId &&
                    !p.IsDeleted);

            if (placementDrive == null)
                throw new KeyNotFoundException(
                    $"Placement drive with id {dto.PlacementDriveId} not found.");

            // 3. Check drive status
            if (placementDrive.Status != PlacementDriveStatus.Open)
            {
                throw new InvalidOperationException(
                    "Applications are not open for this placement drive.");
            }

            // 4. Check application deadline
            if (DateTime.UtcNow > placementDrive.ApplicationDeadline)
            {
                throw new InvalidOperationException(
                    "The application deadline for this placement drive has passed.");
            }

            // 5. Check duplicate application
            var alreadyApplied = await _applicationRepo.TableNoTracking
                .AnyAsync(a =>
                    a.StudentId == student.Id &&
                    a.PlacementDriveId == placementDrive.Id &&
                    !a.IsDeleted);

            if (alreadyApplied)
            {
                throw new InvalidOperationException(
                    "You have already applied for this placement drive.");
            }

            // 6. Check graduation year
            var education = await _educationRepo.TableNoTracking
                .Where(e =>
                    e.StudentId == student.Id &&
                    !e.IsDeleted &&
                    e.EducationType == EducationType.Undergraduate)
                .OrderByDescending(e => e.YearOfPassing)
                .FirstOrDefaultAsync();

            if (education == null)
            {
                throw new InvalidOperationException(
                    "Undergraduate education details not found.");
            }

            if (education.YearOfPassing != placementDrive.GraduationYear)
            {
                throw new InvalidOperationException(
                    "You are not eligible based on the required graduation year.");
            }

            // 7. Check CGPA
            if (education.PercentageOrCgpa < placementDrive.MinimumCgpa)
            {
                throw new InvalidOperationException(
                    $"Minimum CGPA required is {placementDrive.MinimumCgpa}.");
            }

            // 8. Check backlogs
            if (education.Backlogs > placementDrive.MaximumBacklogs)
            {
                throw new InvalidOperationException(
                    $"Maximum allowed backlogs are {placementDrive.MaximumBacklogs}.");
            }

            // 9. Check eligible department
            var eligibleDepartment = placementDrive.PlacementDriveDepartments
                .Any(pd =>
                    pd.DepartmentId == student.DepartmentId &&
                    !pd.IsDeleted);

            if (!eligibleDepartment)
            {
                throw new InvalidOperationException(
                    "Your department is not eligible for this placement drive.");
            }

            var year = DateTime.UtcNow.Year;

            var applicationNumber = await GenerateApplicationNumberAsync(year);

            // 10. Create application
            var application = new Application
            {
                ApplicationNumber = applicationNumber,
                StudentId = student.Id,
                PlacementDriveId = placementDrive.Id,
                AppliedDate = DateTime.UtcNow,
                Status = ApplicationStatus.Applied,
                CreatedBy = UserId,
                CreatedDate = DateTime.UtcNow
            };

            await _applicationRepo.AddAsync(application);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Student {StudentId} applied for placement drive {PlacementDriveId}.",
                student.Id,
                placementDrive.Id);

            var emailBody = BuildApplicationSubmittedEmail((student.User.FirstName+" "+student.User.LastName), application.ApplicationNumber,
                             placementDrive.Company.Name, placementDrive.JobTitle, application.AppliedDate);

            await _emailService.SendEmailAsync(
                student.User.Email, $"Application Submitted - {application.ApplicationNumber}", emailBody);

            // 11. Response
            return new ApplicationResponseDto
            {
                ApplicationNumber = application.ApplicationNumber,
                ApplicationId = application.Id,
                StudentId = student.Id,
                StudentName = student.User.FirstName + " " + student.User.LastName,
                RegisterNumber=student.RegisterNumber,
                Email=student.User.Email,
                Department=student.Department.Name,
                PlacementDriveId = placementDrive.Id,
                JobTitle = placementDrive.JobTitle,
                CompanyId=placementDrive.CompanyId,
                CompanyName = placementDrive.Company.Name,
                AppliedDate = application.AppliedDate,
                ApplicationStatus = application.Status
            };
        }

        public async Task<ApplicationDto> GetApplicationById(int id) {


            var application = await _applicationRepo.TableNoTracking
                              .Where(a => a.Id == id && a.IsDeleted != true)
                               .Select(a => new ApplicationDto
                               {
                                   ApplicationId = a.Id,
                                   ApplicationNumber = a.ApplicationNumber,
                                   StudentId = a.StudentId,
                                   FirstName = a.Student.User.FirstName,
                                   LastName = a.Student.User.LastName,
                                   RegisterNumber = a.Student.RegisterNumber,
                                   Email = a.Student.User.Email,
                                   Department = a.Student.Department.Name,
                                   PlacementDriveId = a.PlacementDriveId,
                                   JobTitle = a.PlacementDrive.JobTitle,
                                   Location = a.PlacementDrive.Location,
                                   CompanyId = a.PlacementDrive.CompanyId,
                                   CompanyName = a.PlacementDrive.Company.Name,
                                   IndustryType = a.PlacementDrive.Company.IndustryType,
                                   OtherIndustry = a.PlacementDrive.Company.OtherIndustry,
                                   Website = a.PlacementDrive.Company.Website,
                                   CompanyEmail = a.PlacementDrive.Company.Email,
                                   CompanyPhone = a.PlacementDrive.Company.Phone,
                                   CompanyDescription = a.PlacementDrive.Company.Description,
                                   AppliedDate = a.AppliedDate,
                                   ApplicationStatus = a.Status
                               }).FirstOrDefaultAsync();

            if (application == null)

                throw new KeyNotFoundException($"Application Id {id} not found");

            return application;


        }

        public async Task<List<ApplicationDto>> GetMyApplication(int UserId) {

            var applications = await _applicationRepo.TableNoTracking
                             .Where(a => a.Student.UserId == UserId && !a.IsDeleted)
                            .Select(a => new ApplicationDto
                            {
                                ApplicationId = a.Id,
                                ApplicationNumber = a.ApplicationNumber,
                                StudentId = a.StudentId,
                                FirstName = a.Student.User.FirstName,
                                LastName = a.Student.User.LastName,
                                RegisterNumber = a.Student.RegisterNumber,
                                Email = a.Student.User.Email,
                                Department=a.Student.Department.Name,
                                PlacementDriveId = a.PlacementDriveId,
                                JobTitle = a.PlacementDrive.JobTitle,
                                Location = a.PlacementDrive.Location,
                                CompanyId=a.PlacementDrive.CompanyId,
                                CompanyName = a.PlacementDrive.Company.Name,
                                IndustryType = a.PlacementDrive.Company.IndustryType,
                                OtherIndustry = a.PlacementDrive.Company.OtherIndustry,
                                Website = a.PlacementDrive.Company.Website,
                                CompanyEmail = a.PlacementDrive.Company.Email,
                                CompanyPhone = a.PlacementDrive.Company.Phone,
                                CompanyDescription = a.PlacementDrive.Company.Description,
                                AppliedDate = a.AppliedDate,
                                ApplicationStatus = a.Status
                            }).ToListAsync();


            if (applications.Count == 0)

                throw new KeyNotFoundException("You currently have no applications.");

            return applications;

        }

        public async Task<PagedResult<ApplicationResponseDto>> GetAllApplication(ApplicationQueryParameters request)
        {
            
                var query = _applicationRepo.TableNoTracking
                                .Where(a => !a.IsDeleted);

                if (query == null)

                    throw new KeyNotFoundException("No applications found.");


            query = ApplyApplicationQuery(query, request);


            var resultQuery = query.Select(a => new ApplicationResponseDto
            {
                ApplicationId = a.Id,
                ApplicationNumber = a.ApplicationNumber,
                StudentId = a.StudentId,
                StudentName = a.Student.User.FirstName + " " + a.Student.User.LastName,
                RegisterNumber = a.Student.RegisterNumber,
                Email = a.Student.User.Email,
                Department = a.Student.Department.Name,
                PlacementDriveId = a.PlacementDriveId,
                JobTitle = a.PlacementDrive.JobTitle,
                CompanyId = a.PlacementDrive.CompanyId,
                CompanyName = a.PlacementDrive.Company.Name,
                AppliedDate = a.AppliedDate,
                ApplicationStatus = a.Status
            });

              return await resultQuery.ToPagedResultAsync(request);     
        }

        private IQueryable<Application> ApplyApplicationQuery(IQueryable<Application> query,
        ApplicationQueryParameters request)
        {

            if (request.Status.HasValue)
            {

                query = query.Where(a => a.Status == request.Status);


            }
            if (!string.IsNullOrWhiteSpace(request.ApplicationNumber))
            {

                var applicationNo = request.ApplicationNumber.Trim();

                query = query.Where(a => a.ApplicationNumber.Contains(applicationNo));


            }
            if (!string.IsNullOrWhiteSpace(request.RegisterNumber))
            {

                var regNo = request.RegisterNumber.Trim();

                query = query.Where(a => a.Student.RegisterNumber.Contains(regNo));


            }
            //Created Date From
            if (request.AppliedFrom.HasValue)
            {
                var fromDate = request.AppliedFrom.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(a => a.AppliedDate >= fromDate);

            }

            // Created Date To
            if (request.AppliedTo.HasValue)
            {
                var toDate = request.AppliedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);

                query = query.Where(a => a.AppliedDate < toDate);

            }

            if (request.DepartmentId.HasValue)
            {
                query = query.Where(a => a.PlacementDrive.PlacementDriveDepartments.Any(pd => pd.DepartmentId == request.DepartmentId.Value));
            }

            var sortOptions = new Dictionary<string, Expression<Func<Application, object?>>>
            {
                ["AppliedDate"] = a => a.AppliedDate

            };

            query = query.ApplySorting(request, sortOptions, defaultSort: e => (object?)e.Id);

            return query;


        }

        public async Task<byte[]> DownloadApplicationsAsync(ApplicationQueryParameters request)
        {
            var query = _applicationRepo.TableNoTracking
                .Where(a => !a.IsDeleted);

            // Reuse search, filter and sorting
            query = ApplyApplicationQuery(query, request);

            var applications = await query
                .Select(a => new DownloadApplication
                {

                    ApplicationNumber = a.ApplicationNumber,
                    FirstName = a.Student.User.FirstName,
                    LastName = a.Student.User.LastName,
                    RegisterNumber = a.Student.RegisterNumber,
                    Email = a.Student.User.Email,
                    Department = a.Student.Department.Name,
                    JobTitle = a.PlacementDrive.JobTitle,
                    CompanyName = a.PlacementDrive.Company.Name,
                    Location = a.PlacementDrive.Location,
                    IndustryType = a.PlacementDrive.Company.IndustryType,
                    OtherIndustry = a.PlacementDrive.Company.OtherIndustry,
                    Website = a.PlacementDrive.Company.Website,
                    CompanyEmail = a.PlacementDrive.Company.Email,
                    CompanyPhone = a.PlacementDrive.Company.Phone,
                    CompanyDescription = a.PlacementDrive.Company.Description,
                    AppliedDate = a.AppliedDate,
                    ApplicationStatus = a.Status,
                    //Education Details
                }).ToListAsync();

            if (applications.Count == 0)
                throw new KeyNotFoundException(
                    "No applications found for the selected criteria.");

            using var workbook = new XLWorkbook();

            var worksheet = workbook.Worksheets.Add("Applications");

            // Header
            worksheet.Cell(1, 1).Value = "Application Number";
            worksheet.Cell(1, 2).Value = "First Name";
            worksheet.Cell(1, 3).Value = "Last Name";
            worksheet.Cell(1, 4).Value = "Register Number";
            worksheet.Cell(1, 5).Value = "Email";
            worksheet.Cell(1, 6).Value = "Department";
            worksheet.Cell(1, 7).Value = "Job Title";
            worksheet.Cell(1, 8).Value = "Company Name";
            worksheet.Cell(1, 9).Value = "Location";
            worksheet.Cell(1, 10).Value = "Industry Type";
            worksheet.Cell(1, 11).Value = "OtherIndustry";
            worksheet.Cell(1, 12).Value = "Website ";
            worksheet.Cell(1, 13).Value = "CompanyEmail";
            worksheet.Cell(1, 14).Value = "CompanyPhone";
            worksheet.Cell(1, 15).Value = "CompanyDescription";
            worksheet.Cell(1, 16).Value = "Applied Date";
            worksheet.Cell(1, 17).Value = "Status";

            // Data
            for (int i = 0; i < applications.Count; i++)
            {
                var row = i + 2;
                var application = applications[i];

                
                worksheet.Cell(row, 1).Value = application.ApplicationNumber;
                worksheet.Cell(row, 2).Value = application.FirstName;
                worksheet.Cell(row, 3).Value = application.LastName;
                worksheet.Cell(row, 4).Value = application.RegisterNumber;
                worksheet.Cell(row, 5).Value = application.Email;
                worksheet.Cell(row, 6).Value = application.Department;
                worksheet.Cell(row, 7).Value = application.JobTitle;
                worksheet.Cell(row, 8).Value = application.CompanyName;
                worksheet.Cell(row, 9).Value = application.Location;
                worksheet.Cell(row, 10).Value = application.IndustryType.ToString();
                worksheet.Cell(row, 11).Value = application.OtherIndustry;
                worksheet.Cell(row, 12).Value = application.Website;
                worksheet.Cell(row, 13).Value = application.CompanyEmail;
                worksheet.Cell(row, 14).Value = application.CompanyPhone;
                worksheet.Cell(row, 15).Value = application.CompanyDescription;
                worksheet.Cell(row, 16).Value = application.AppliedDate;
                worksheet.Cell(row, 17).Value = application.ApplicationStatus.ToString();

            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }

        public async Task UpdateApplicationAsync(UpdateApplicationRequestDto dto,int id, int UpdatedBy) {


            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var application = await _applicationRepo.Table.Where(a => a.Id == id && !a.IsDeleted)
                             .FirstOrDefaultAsync();

            if (application == null) {

                throw new KeyNotFoundException($"Application Id {id} not found");
            
            }
            if (dto.ApplicationStatus.HasValue)
            {
                application.Status = dto.ApplicationStatus.Value;

            }
            if (dto.IsActive.HasValue)
            {
                application.IsActive = dto.IsActive.Value;

            }
            application.UpdatedDate = DateTime.UtcNow;

            application.UpdatedBy = UpdatedBy;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
             "Application  {ApplicationId} updated by {UpdatedBy}.", id, UpdatedBy);

        }

        public async Task DeleteApplicationAsync(int id,int DeletedBy) {

            var application = await _applicationRepo.Table
                             .Where(a=>a.Id==id && !a.IsDeleted)
                             .FirstOrDefaultAsync();

            if (application == null)
            {

                throw new KeyNotFoundException($"Application Id:{id} not Found");


            }

            application.IsDeleted = true;
            application.IsActive = false;
            application.DeletedDate = DateTime.UtcNow;
            application.DeletedBy = DeletedBy;

            _logger.LogInformation("Application Id:{id} DeletedBy {DeletedBy} Successfully",application.Id,DeletedBy);
        
        }

        private static string BuildApplicationSubmittedEmail(string Name, string applicationNumber, string companyName, string jobTitle,
        DateTime appliedDate)
        {
            return $"""
         <!DOCTYPE html>
          <html>
          <body style="margin:0;padding:0;background:#f4f6f8;font-family:Arial,sans-serif;">

        <div style="
            max-width:600px;
            margin:30px auto;
            background:#ffffff;
            border-radius:12px;
            overflow:hidden;
            box-shadow:0 4px 15px rgba(0,0,0,0.08);">

            <div style="
                background:#1f6feb;
                color:white;
                padding:25px;
                text-align:center;">
                
                <h2 style="margin:0;">
                    Placement Application Submitted
                </h2>
            </div>

            <div style="padding:30px;">

                <h3>Hello {Name},</h3>

                <p>
                    Your placement application has been
                    <strong>successfully submitted</strong>.
                </p>

                <div style="
                    background:#e8f1ff;
                    color:#1f6feb;
                    padding:15px;
                    border-radius:8px;
                    text-align:center;
                    font-size:20px;
                    font-weight:bold;
                    margin:20px 0;">
                    
                    Application No: {applicationNumber}
                </div>

                <table style="width:100%;border-collapse:collapse;">

                    <tr>
                        <td style="padding:10px;font-weight:bold;">
                            Company
                        </td>
                        <td style="padding:10px;">
                            {companyName}
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:10px;font-weight:bold;">
                            Position
                        </td>
                        <td style="padding:10px;">
                            {jobTitle}
                        </td>
                    </tr>

                    <tr>
                        <td style="padding:10px;font-weight:bold;">
                            Applied Date
                        </td>
                        <td style="padding:10px;">
                            {appliedDate:dd MMM yyyy, hh:mm tt}
                        </td>
                    </tr>

                </table>

                <p style="margin-top:25px;">
                    Please keep your application number for future reference.
                </p>

                <p>
                    Regards,<br>
                    <strong>Placement Management Team</strong>
                </p>

            </div>

            <div style="
                background:#f8fafc;
                padding:15px;
                text-align:center;
                font-size:12px;
                color:#777;">
                
                This is an automated email. Please do not reply.
            </div>

        </div>

          </body>
          </html>
      """;
        }
    }
}