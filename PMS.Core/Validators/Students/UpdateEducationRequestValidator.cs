using FluentValidation;
using PMS.Core.DTOs.Students;

public class UpdateEducationRequestValidator : AbstractValidator<UpdateEducationRequestDto>
{
    public UpdateEducationRequestValidator()
    {
        RuleFor(x => x.EducationId)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.EducationType)
            .IsInEnum();

        RuleFor(x => x.Institution)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.PercentageOrCgpa)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.Backlogs)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.YearOfPassing)
            .InclusiveBetween(1900, DateTime.UtcNow.Year);

        RuleFor(x => x.Location)
            .MaximumLength(200)
            .When(x => x.Location != null);
    }
}