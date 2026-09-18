using PMS.Core.DTOs.Dashboard;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PMS.Service.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IRepository<Student> _studentRepo;
        private readonly IRepository<Application> _applicationRepo;
        private readonly IRepository<Department> _departmentRepo;
        private readonly IRepository<Company> _companyRepo;
        private readonly IRepository<PlacementDrive> _placementDriveRepo;

        public DashboardService(
            IRepository<Student> studentRepo,
            IRepository<Application> applicationRepo,
            IRepository<Department> departmentRepo,
            IRepository<Company> companyRepo,
            IRepository<PlacementDrive> placementDriveRepo)
        {
            _studentRepo = studentRepo;
            _applicationRepo = applicationRepo;
            _departmentRepo = departmentRepo;
            _companyRepo = companyRepo;
            _placementDriveRepo = placementDriveRepo;
        }

        public async Task<DashboardResponseDto> GetDashboardAsync(
        DashboardQueryParameters request)
        {
            // =========================================================
            // 1. Student Query
            // =========================================================

            var studentQuery = _studentRepo.TableNoTracking.Where(s => !s.IsDeleted);


            if (request.Gender.HasValue)
            {
                studentQuery = studentQuery.Where(s => s.Gender == request.Gender.Value);

            }

            if (request.PlacementStatus.HasValue)
            {
                studentQuery = studentQuery
                    .Where(s =>
                        s.PlacementStatus == request.PlacementStatus.Value);
            }

            var totalStudents = await studentQuery.CountAsync();

            var placedStudents = await studentQuery
                .CountAsync(s =>
                    s.PlacementStatus == PlacementStatus.Placed);

            var unplacedStudents = await studentQuery
                .CountAsync(s =>
                    s.PlacementStatus != PlacementStatus.Placed);

            var genderWiseCount = await studentQuery
                .GroupBy(s => s.Gender)
                .Select(g => new GenderCountDto
                {
                    Gender = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            // =========================================================
            // 2. Application Query
            // =========================================================

            var applicationQuery = _applicationRepo.TableNoTracking
                .Where(a =>
                    !a.IsDeleted &&
                    !a.Student.IsDeleted);

            if (request.Gender.HasValue)
            {
                applicationQuery = applicationQuery
                    .Where(a =>
                        a.Student.Gender == request.Gender.Value);
            }

            if (request.PlacementStatus.HasValue)
            {
                applicationQuery = applicationQuery
                    .Where(a =>
                        a.Student.PlacementStatus ==
                        request.PlacementStatus.Value);
            }

            var totalApplications = await applicationQuery.CountAsync();

            var appliedApplications = await applicationQuery
                .CountAsync(a =>
                    a.Status == ApplicationStatus.Applied);

            var shortlistedApplications = await applicationQuery
                .CountAsync(a =>
                    a.Status == ApplicationStatus.Shortlisted);

            var selectedApplications = await applicationQuery
                .CountAsync(a =>
                    a.Status == ApplicationStatus.Selected);

            // =========================================================
            // 3. Master Counts
            // =========================================================

            var totalDepartments = await _departmentRepo.TableNoTracking
                .CountAsync(d => !d.IsDeleted);

            var totalCompanies = await _companyRepo.TableNoTracking
                .CountAsync(c => !c.IsDeleted);

            var totalPlacementDrives =
                await _placementDriveRepo.TableNoTracking
                    .CountAsync(p => !p.IsDeleted);

            // =========================================================
            // 4. Response
            // =========================================================

            return new DashboardResponseDto
            {
                TotalStudents = totalStudents,
                PlacedStudents = placedStudents,
                UnplacedStudents = unplacedStudents,

                TotalApplications = totalApplications,
                AppliedApplications = appliedApplications,
                ShortlistedApplications = shortlistedApplications,
                SelectedApplications = selectedApplications,

                TotalDepartments = totalDepartments,
                TotalCompanies = totalCompanies,
                TotalPlacementDrives = totalPlacementDrives,

                GenderWiseCount = genderWiseCount
            };
        }
    }
}
