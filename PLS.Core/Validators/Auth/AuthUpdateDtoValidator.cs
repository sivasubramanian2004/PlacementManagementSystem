using FluentValidation;
using PMS.Core.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Validators
{
    public class AuthUpdateDtoValidator : AbstractValidator<AuthUpdateDto>
    {
        public AuthUpdateDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required.")
                .MaximumLength(50)
                .WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MaximumLength(50)
                .WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.Password)
                .MinimumLength(8)
                .WithMessage("Password must be at least 8 characters long.");

            RuleFor(x => x.Role)
               .MaximumLength(50)
               .WithMessage("Role cannot exceed 50 characters.");


        }
    }
}
