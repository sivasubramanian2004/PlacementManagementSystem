using FluentValidation;
using PMS.Core.DTOs.Students;

public class UpdateStudentRequestValidator : AbstractValidator<UpdateStudentRequestDto>
{
    public UpdateStudentRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.LastName != null);

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.RegisterNumber)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.RegisterNumber != null);

        RuleFor(x => x.Gender)
            .IsInEnum()
            .When(x => x.Gender.HasValue);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => x.Phone != null);

        RuleFor(x => x.Skills)
            .MaximumLength(500)
            .When(x => x.Skills != null);

        RuleFor(x => x.PlacementStatus)
            .IsInEnum()
            .When(x => x.PlacementStatus.HasValue);

        RuleFor(x => x.Resume)
            .Must(file => file != null && file.Length > 0)
            .WithMessage("Resume file cannot be empty.")
            .When(x => x.Resume != null);

        RuleFor(x => x.ProfilePhoto)
            .Must(file => file != null && file.Length > 0)
            .WithMessage("Profile photo cannot be empty.")
            .When(x => x.ProfilePhoto != null);

        RuleForEach(x => x.Educations)
            .SetValidator(new UpdateEducationRequestValidator());
    }
}