using FluentValidation;
using PMS.Core.DTOs.Applications;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Validators.Applications
{
    public class UpdateApplicationRequestValidator
      : AbstractValidator<UpdateApplicationRequestDto>
    {
        public UpdateApplicationRequestValidator()
        {
            RuleFor(x => x.ApplicationStatus)
                .IsInEnum()
                .When(x => x.ApplicationStatus.HasValue)
                .WithMessage("Invalid application status.");
        }
    }
}
