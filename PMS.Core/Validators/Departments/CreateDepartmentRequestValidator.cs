using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;
using PMS.Core.DTOs.Departments;
namespace PMS.Core.Validators.Departments
{
    public class CreateDepartmentRequestValidator
        : AbstractValidator<CreateDepartmentRequestDto>
    {
        public CreateDepartmentRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Code)
                .NotEmpty()
                .MaximumLength(20);
        }
    }
}
