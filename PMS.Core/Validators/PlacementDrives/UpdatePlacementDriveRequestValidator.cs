using FluentValidation;
using PMS.Core.DTOs.PlacementDrives;        
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Validators.PlacementDrives
{
    public class UpdatePlacementDriveRequestValidator
    : AbstractValidator<UpdatePlacementDriveRequestDto>
    {
        public UpdatePlacementDriveRequestValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .When(x => x.CompanyId.HasValue);

            RuleFor(x => x.JobTitle)
                .MaximumLength(150)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .When(x => x.JobTitle != null)
                .WithMessage("Job title cannot be empty or contain only spaces.");

            RuleFor(x => x.JobDescription)
                .MaximumLength(2000)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .When(x => x.JobDescription != null)
                .WithMessage("Job description cannot be empty or contain only spaces.");

            RuleFor(x => x.EmploymentType)
                .IsInEnum()
                .When(x => x.EmploymentType.HasValue);

            RuleFor(x => x.WorkMode)
                .IsInEnum()
                .When(x => x.WorkMode.HasValue);

            RuleFor(x => x.Location)
                .MaximumLength(250)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .When(x => x.Location != null)
                .WithMessage("Location cannot be empty or contain only spaces.");

            RuleFor(x => x.MinimumCgpa)
                .InclusiveBetween(0, 10)
                .When(x => x.MinimumCgpa.HasValue);

            RuleFor(x => x.MaximumBacklogs)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaximumBacklogs.HasValue);

            RuleFor(x => x.GraduationYear)
                .InclusiveBetween(2000, 2100)
                .When(x => x.GraduationYear.HasValue);

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Salary.HasValue);

            RuleFor(x => x.RequiredSkills)
                .MaximumLength(1000)
                .When(x => x.RequiredSkills != null);

            RuleFor(x => x.DepartmentIds)
                .NotEmpty()
                .When(x => x.DepartmentIds != null)
                .WithMessage("At least one department must be selected.");

            RuleFor(x => x.DepartmentIds)
                .Must(ids => ids!.Distinct().Count() == ids.Count)
                .When(x => x.DepartmentIds != null)
                .WithMessage("DepartmentIds must not contain duplicates.");

            RuleForEach(x => x.DepartmentIds)
                .GreaterThan(0)
                .When(x => x.DepartmentIds != null);
        }
    }
}
