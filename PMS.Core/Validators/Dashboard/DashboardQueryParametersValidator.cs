using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using PMS.Core.DTOs.Dashboard;
namespace PMS.Core.Validators.Dashboard
{
        public class DashboardQueryParametersValidator
            : AbstractValidator<DashboardQueryParameters>
        {
            public DashboardQueryParametersValidator()
            {
                RuleFor(x => x.Gender)
                    .IsInEnum()
                    .When(x => x.Gender.HasValue)
                    .WithMessage("Invalid gender.");

                RuleFor(x => x.PlacementStatus)
                    .IsInEnum()
                    .When(x => x.PlacementStatus.HasValue)
                    .WithMessage("Invalid placement status.");
            }
        }
    
}
