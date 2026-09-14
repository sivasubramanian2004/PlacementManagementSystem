using FluentValidation;
using PMS.Core.DTOs.Companies;
using PMS.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Validators.Companies
{
    public class CreateCompanyRequestValidator
      : AbstractValidator<CreateCompanyRequestDto>
    {
        public CreateCompanyRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150)
                .Must(x => x.Trim().Length > 0)
                .WithMessage("Company name cannot contain only spaces.");

            RuleFor(x => x.IndustryType)
                .IsInEnum()
                .WithMessage("Invalid industry type.");

            // OtherIndustry is required only when IndustryType = Others
            RuleFor(x => x.OtherIndustry)
                .NotEmpty()
                .MaximumLength(100)
                .When(x => x.IndustryType == IndustryType.Others)
                .WithMessage(
                    "OtherIndustry is required when IndustryType is Others.");

            // OtherIndustry should not be provided for predefined industries
            RuleFor(x => x.OtherIndustry)
                .NotEmpty()
                .MaximumLength(100)
                .Must(x => !string.IsNullOrWhiteSpace(x))
                .When(x => x.IndustryType == IndustryType.Others)
                .WithMessage("OtherIndustry is required when IndustryType is Others.");

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
                .MaximumLength(1000);

            RuleFor(x => x.Location)
                .MaximumLength(250);
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
