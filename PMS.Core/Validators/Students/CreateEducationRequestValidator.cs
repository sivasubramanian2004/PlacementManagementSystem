using FluentValidation;
using PMS.Core.DTOs.Departments;
using PMS.Core.DTOs.Students;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Validators.Students
{
    public class CreateEducationRequestDtoValidator : AbstractValidator<CreateEducationRequestDto>
    {
        public CreateEducationRequestDtoValidator()
        {
            RuleFor(x => x.EducationType)
                .NotEmpty()
                .WithMessage("EducationType is required.");

            RuleFor(x => x.Institution)
                .NotEmpty()
                .WithMessage("Institution is required.")
                .MaximumLength(200)
                .WithMessage("Institution cannot exceed 200  characters.");


            RuleFor(x => x.PercentageOrCgpa)
                .NotEmpty()
                .PrecisionScale(10, 2, true)
                .WithMessage("Percentage or CGPA is required.");

            RuleFor(x => x.Backlogs)
                .NotEmpty()
                .WithMessage("Backlogs is required.");

            RuleFor(x => x.YearOfPassing)
                .NotEmpty()
                .WithMessage("Year of Passing is required.");


            RuleFor(x => x.Location)
                .MaximumLength(500)
                .WithMessage("Location cannot exceed 200  characters.");
        }
    }
}
