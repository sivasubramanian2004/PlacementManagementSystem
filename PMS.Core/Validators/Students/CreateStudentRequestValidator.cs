using FluentValidation;
using PMS.Core.DTOs.Students;
using PMS.Core.Helpers;

namespace PMS.Core.Validators.Student;

public class CreateStudentRequestValidator : AbstractValidator<CreateStudentRequestDto>
{
    public CreateStudentRequestValidator()
    {
        // Email
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        // Department
        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .WithMessage("DepartmentId must be greater than 0.");

        // Register Number
        RuleFor(x => x.RegisterNumber)
            .NotEmpty()
            .MaximumLength(100)
            .Must(x => x.Trim().Length > 0)
            .WithMessage("Register number cannot contain only spaces.");

        // Name
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .Must(x => x.Trim().Length > 0)
            .WithMessage("Name cannot contain only spaces.");

        // Gender
        RuleFor(x => x.Gender)
            .IsInEnum()
            .WithMessage("Invalid Gender value.");

        // Phone - Optional
        RuleFor(x => x.Phone)
            .MaximumLength(30)
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        // Skills
        RuleFor(x => x.Skills)
            .MaximumLength(500);

        // Placement Status
        RuleFor(x => x.PlacementStatus)
            .IsInEnum()
            .WithMessage("Invalid PlacementStatus value.");

        // Resume
        RuleFor(x => x.Resume)
            .NotNull()
            .WithMessage("Resume is required.");

        // Profile Photo - Optional
        RuleFor(x => x.ProfilePhoto)
            .Must(file =>
                file == null ||
                file.Length > 0)
            .WithMessage("Profile photo cannot be empty.");
    }
}