using Business.Models;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class BusinessAccountValidator : AbstractValidator<BusinessAccount>, IValidator<Account>
    {
        public BusinessAccountValidator()
        {
            RuleFor(a => a.Name).NotEmpty().WithMessage("Please enter a business name");
            RuleFor(a => a.BusinessType).NotNull().WithMessage("Please select a business type");
        }

        public FluentValidation.Results.ValidationResult Validate(Business.Models.Account instance)
        {
            return base.Validate((Business.Models.BusinessAccount)instance);
        }
    }
}
