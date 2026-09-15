using FluentValidation;
using PMS.Core.DTOs.PlacementDrives;
using PMS.Core.Helpers;

namespace PMS.Core.Validators.PlacementDrives
{
    public class CreatePlacementDriveRequestValidator : AbstractValidator<CreatePlacementDriveRequestDto>
    {
        public CreatePlacementDriveRequestValidator()
        {
            RuleFor(x => x.CompanyId)
                .GreaterThan(0)
                .WithMessage("CompanyId must be greater than 0.");

            RuleFor(x => x.JobTitle)
                .NotEmpty()
                .MaximumLength(150)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Job title cannot contain only spaces.");

            RuleFor(x => x.JobDescription)
                .NotEmpty()
                .MaximumLength(2000)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Job description cannot contain only spaces.");

            RuleFor(x => x.EmploymentType)
                .IsInEnum()
                .WithMessage("Invalid employment type.");

            RuleFor(x => x.WorkMode)
                .IsInEnum()
                .WithMessage("Invalid work mode.");

            RuleFor(x => x.Location)
                .NotEmpty()
                .MaximumLength(250)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .WithMessage("Location cannot contain only spaces.");

            RuleFor(x => x.MinimumCgpa)
                .InclusiveBetween(0, 10)
                .WithMessage("Minimum CGPA must be between 0 and 10.");

            RuleFor(x => x.MaximumBacklogs)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Maximum backlogs cannot be negative.");

            RuleFor(x => x.GraduationYear)
                .InclusiveBetween(2000, 2100)
                .WithMessage("Invalid graduation year.");

            RuleFor(x => x.Salary)
                .GreaterThanOrEqualTo(0)
                .When(x => x.Salary.HasValue)
                .WithMessage("Salary cannot be negative.");

            RuleFor(x => x.RequiredSkills)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.RequiredSkills));

            RuleFor(x => x.ApplicationDeadline)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Application deadline must be in the future.");

            RuleFor(x => x.DriveDate)
                .GreaterThanOrEqualTo(x => x.ApplicationDeadline)
                .When(x => x.DriveDate.HasValue)
                .WithMessage(
                    "Drive date must be after the application deadline.");

            RuleFor(x => x.DepartmentIds)
                .NotEmpty()
                .WithMessage("At least one department must be selected.");

            RuleFor(x => x.DepartmentIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage("DepartmentIds must not contain duplicates.");

            RuleForEach(x => x.DepartmentIds)
                .GreaterThan(0)
                .WithMessage("DepartmentId must be greater than 0.");
        }
    }
}