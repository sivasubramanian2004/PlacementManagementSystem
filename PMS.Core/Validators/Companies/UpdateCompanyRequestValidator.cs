using FluentValidation;
using PMS.Core.DTOs.Companies;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Validators.Companies
{

        public class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequestDto>
        {
            public UpdateCompanyRequestValidator()
            {
                RuleFor(x => x.Name)
                    .MaximumLength(150)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .When(x => x.Name != null)
                    .WithMessage("Company name cannot be empty or contain only spaces.");

                RuleFor(x => x.IndustryType)
                    .IsInEnum()
                    .When(x => x.IndustryType.HasValue)
                    .WithMessage("Invalid industry type.");

                RuleFor(x => x.OtherIndustry)
                    .MaximumLength(100)
                    .Must(x => !string.IsNullOrWhiteSpace(x))
                    .When(x => x.OtherIndustry != null)
                    .WithMessage("OtherIndustry cannot be empty or contain only spaces.");

                RuleFor(x => x.Website)
                    .Must(BeValidUrl)
                    .When(x => !string.IsNullOrWhiteSpace(x.Website))
                    .WithMessage("Website must be a valid URL.");

                RuleFor(x => x.Email)
                    .EmailAddress()
                    .MaximumLength(255)
                    .When(x => !string.IsNullOrWhiteSpace(x.Email))
                    .WithMessage("Invalid company email address.");

                RuleFor(x => x.Phone)
                    .MaximumLength(30)
                    .When(x => !string.IsNullOrWhiteSpace(x.Phone));

                RuleFor(x => x.Description)
                    .MaximumLength(1000)
                    .When(x => x.Description != null);

                RuleFor(x => x.Location)
                    .MaximumLength(250)
                    .When(x => x.Location != null);
            }

            private static bool BeValidUrl(string? url)
            {
                return Uri.TryCreate(
                    url,
                    UriKind.Absolute,
                    out var result)
                    && (result.Scheme == Uri.UriSchemeHttp ||
                        result.Scheme == Uri.UriSchemeHttps);
            }
        }
    
}
