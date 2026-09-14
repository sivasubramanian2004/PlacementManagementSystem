using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PMS.Core.DTOs.Companies;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using PMS.Data.Repositories;
using PMS.Data.UnitOfWork; 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;


namespace PMS.Service.Companies
{
    public  class CompanyService : ICompanyService
    {
       private readonly IRepository<Company> _companyRepo;
       private readonly ILogger<CompanyService> _logger;

       private readonly IUnitOfWork _unitOfWork;

        public CompanyService(IRepository<Company> companyRepo, ILogger<CompanyService> logger, IUnitOfWork unitOfWork)
        {
            _companyRepo = companyRepo;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        public async Task<CompanyResponseDto> InsertAsync(CreateCompanyRequestDto dto, int CreatedBy) { 
        
        
           var Company=await _companyRepo.TableNoTracking
                       .Where(x => x.Name == dto.Name)
                       .FirstOrDefaultAsync();

           if(Company!=null)
                     throw new KeyNotFoundException($"Company with name {dto.Name} already exists.");

            var newCompany = new Company
            {

                Name = dto.Name.Trim(),
                IndustryType = dto.IndustryType,
                OtherIndustry = dto.OtherIndustry,
                Website = dto.Website,
                Email = dto.Email,
                Phone = dto.Phone,
                Description = dto.Description,
                Location = dto.Location,
                CreatedBy = CreatedBy
            };

            var result=await _companyRepo.InsertAsync(newCompany);

            _logger.LogInformation("Company {dto.Name} Created Successfully", dto.Name);

            return new CompanyResponseDto
            {

                Name = result.Name,
                IndustryType = result.IndustryType,
                Email = result.Email

            };
        }

        public async Task<PagedResult<CompanyBasicDto>> GetCompanyAsnyc(CompanyQueryParameters request)
        {

            var query = _companyRepo.TableNoTracking
                         .Where(c => c.IsDeleted != true);

            if (query == null)
            {
                throw new KeyNotFoundException("No Company found.");
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.Trim();

                query = query.Where(c =>
                    c.Name.Contains(searchTerm) ||
                    c.Email!.Contains(searchTerm) ||
                    c.Location!.Contains(searchTerm));
            }
            if (request.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == request.IsActive.Value);
            }
            //Created Date From
            if (request.CreatedFrom.HasValue)
            {
                var fromDate = request.CreatedFrom.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(c => c.CreatedDate >= fromDate);

            }

            // Created Date To
            if (request.CreatedTo.HasValue)
            {
                var toDate = request.CreatedTo.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);

                query = query.Where(c => c.CreatedDate < toDate);

            }

            var sortOptions = new Dictionary<string, Expression<Func<Company, object?>>>
            {
                ["Name"] = c => c.Name,
                ["CreatedDate"] = c => c.CreatedDate
            };

            query = query.ApplySorting(request, sortOptions, defaultSort: e => (object?)e.Id);

            var result = query.Select(c => new CompanyBasicDto
            {
                Id = c.Id,
                Name = c.Name,
                IndustryType = c.IndustryType,
                OtherIndustry = c.OtherIndustry,
                Website = c.Website,
                Email = c.Email,
                Phone = c.Phone,
                Description = c.Description,
                Location = c.Location,
                IsActive = c.IsActive,
                CreatedDate = c.CreatedDate

            });
            return await result.ToPagedResultAsync(request);
        }


        public async Task UpdateAsync(
    int id,
    UpdateCompanyRequestDto dto,
    int updatedBy)
        {
            var company = await _companyRepo.Table
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (company == null)
                throw new KeyNotFoundException(
                    $"Company with id {id} not found.");

            // Name
            if (dto.Name != null)
            {
                var name = dto.Name.Trim();

                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException(
                        "Company name cannot be empty or contain only spaces.");

                var existingCompany = await _companyRepo.TableNoTracking
                    .FirstOrDefaultAsync(c =>
                        c.Id != id &&
                        !c.IsDeleted &&
                        c.Name == name);

                if (existingCompany != null)
                {
                    throw new InvalidOperationException(
                        $"A company with the name '{name}' already exists.");
                }

                company.Name = name;
            }

            // Industry
            if (dto.IndustryType.HasValue)
            {
                company.IndustryType = dto.IndustryType.Value;

                if (dto.IndustryType.Value != IndustryType.Others)
                {
                    company.OtherIndustry = null;
                }
            }

            // OtherIndustry
            if (dto.OtherIndustry != null)
            {
                if (string.IsNullOrWhiteSpace(dto.OtherIndustry))
                    throw new ArgumentException(
                        "OtherIndustry cannot be empty.");

                if (company.IndustryType != IndustryType.Others)
                    throw new ArgumentException(
                        "OtherIndustry can only be provided when IndustryType is Others.");

                company.OtherIndustry = dto.OtherIndustry.Trim();
            }

            // Website
            if (dto.Website != null)
                company.Website = string.IsNullOrWhiteSpace(dto.Website)
                    ? null
                    : dto.Website.Trim();

            // Email
            if (dto.Email != null)
                company.Email = string.IsNullOrWhiteSpace(dto.Email)
                    ? null
                    : dto.Email.Trim();

            // Phone
            if (dto.Phone != null)
                company.Phone = string.IsNullOrWhiteSpace(dto.Phone)
                    ? null
                    : dto.Phone.Trim();

            // Description
            if (dto.Description != null)
                company.Description = string.IsNullOrWhiteSpace(dto.Description)
                    ? null
                    : dto.Description.Trim();

            // Location
            if (dto.Location != null)
                company.Location = string.IsNullOrWhiteSpace(dto.Location)
                    ? null
                    : dto.Location.Trim();

            // IsActive
            if (dto.IsActive.HasValue)
                company.IsActive = dto.IsActive.Value;

            // Audit
            company.UpdatedBy = updatedBy;
            company.UpdatedDate = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Company with id {id} updated successfully.", id);
        }


        public async Task DeleteAsync(int id, int deletedBy)
        {
            var company = await _companyRepo.Table
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    !c.IsDeleted);

            if (company == null)
                throw new KeyNotFoundException(
                    $"Company with id {id} not found.");

            company.IsDeleted = true;
            company.DeletedDate = DateTime.UtcNow;
            company.DeletedBy = deletedBy;

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "Company {CompanyId} - {CompanyName} deleted by {DeletedBy}",
                company.Id,
                company.Name,
                deletedBy);
        }
    }
    
}
