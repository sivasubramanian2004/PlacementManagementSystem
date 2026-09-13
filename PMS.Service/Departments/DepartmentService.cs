using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Core.DTOs.Departments;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace PMS.Service.Departments
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _departmentRepo;
        private ILogger<DepartmentService> _logger;
        public DepartmentService(IRepository<Department> departmentRepo, ILogger<DepartmentService> logger)
        {

            _departmentRepo = departmentRepo;
            _logger = logger;

        }

        public async Task<DepartmentResponseDto> InsertAsync(CreateDepartmentRequestDto dto, int CreatedBy)
        {

            var existsDepartment = await _departmentRepo.Table.FirstOrDefaultAsync(d => d.Name == dto.Name || d.Code == dto.Code && d.IsDeleted != true);
            if (existsDepartment != null)
                throw new InvalidOperationException($"Department {dto.Name} or {dto.Code} already existed");
            var department = new Department
            {
                Name = dto.Name.Trim().ToUpper(),
                Code = dto.Code.Trim().ToUpper(),
                CreatedDate = DateTime.Now,
                CreatedBy = CreatedBy,

            };
            var result = await _departmentRepo.InsertAsync(department);
            _logger.LogInformation($"Department {dto.Name} Added Succesfully");

            return new DepartmentResponseDto
            {
                DepartmentId = result.Id,
                Name = result.Name,
                Code = result.Code,
                IsActive = result.IsActive
            };

        }

        public async Task DeleteAsync(int id, int DeletedBy)
        {
            var department = await _departmentRepo.Table.
                             FirstOrDefaultAsync(d => d.Id == id && d.IsDeleted != true);
            if (department == null)
                throw new KeyNotFoundException($"No Department found with ID {id}");

            department.IsDeleted = true;
            department.DeletedDate = DateTime.UtcNow;
            department.DeletedBy = DeletedBy;
            var result = await _departmentRepo.UpdateAsync(department);
            _logger.LogInformation($"Deleted Department {department.Name}");
        }

        public async Task UpdateAsync(int id, CreateDepartmentRequestDto dto, int UpdatedBy)
        {
            var department = await _departmentRepo.Table.
                             FirstOrDefaultAsync(d => d.Id == id && d.IsDeleted != true);
            if (department == null)
                throw new KeyNotFoundException($"No Department found with ID {id}");

            var code = dto.Code.Trim().ToUpperInvariant();
            var name = dto.Name.Trim().ToUpperInvariant();

            // Check duplicate Code
            var codeExists = await _departmentRepo.TableNoTracking
                             .AnyAsync(d => d.Code == code && d.Id != id && d.IsDeleted != true);
            if (codeExists)
                throw new InvalidOperationException(
                    $"Department code '{code}' already exists.");
            department.Name = name;
            department.Code = code;
            department.UpdatedDate = DateTime.UtcNow;
            department.UpdatedBy = UpdatedBy;
            var result = await _departmentRepo.UpdateAsync(department);
            _logger.LogInformation($"Updated Department {department.Name}");
        }
        public async Task<PagedResult<DepartmentResponseDto>> GetDepartmentsAsync(DepartmentQueryParameters request) {

            var query = _departmentRepo.TableNoTracking
                        .Where(d => d.IsDeleted != true);

            //Global Searching across multiple fields such as firstname, lastname, email, role
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();

                query = query.Where(d =>
                        d.Name.Contains(searchTerm) ||
                        d.Code.Contains(searchTerm));

            }
            if (request.IsActive.HasValue)
            {
                query = query.Where(d => d.IsActive == request.IsActive.Value);
            }
            var sortOptions = new Dictionary<string, Expression<Func<Department, object?>>>
            {
                ["name"] = d => d.Name,
                ["code"] = d => d.Code
            };
            query = query.ApplySorting(request, sortOptions, defaultSort: e => (object?)e.Id);
            var resultquery = query.Select(d => new DepartmentResponseDto
            {
                DepartmentId = d.Id,
                Name = d.Name,
                Code = d.Code,
                IsActive = d.IsActive

            });

            return await resultquery.ToPagedResultAsync(request);
        }
    }
}
