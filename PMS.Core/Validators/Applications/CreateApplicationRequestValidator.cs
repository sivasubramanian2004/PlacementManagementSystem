using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using PMS.Core.DTOs.Applications;
namespace PMS.Core.Validators.Applications
{
   
    namespace PMS.Core.Validators.Application
    {
        public class CreateApplicationRequestValidator
            : AbstractValidator<CreateApplicationRequestDto>
        {
            public CreateApplicationRequestValidator()
            {
                RuleFor(x => x.PlacementDriveId)
                    .GreaterThan(0)
                    .WithMessage("PlacementDriveId must be greater than 0.");
            }
        }
    }
}
