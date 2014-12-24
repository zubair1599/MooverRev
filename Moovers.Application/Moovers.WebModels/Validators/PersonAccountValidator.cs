using Business.Models;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class PersonAccountValidator : AbstractValidator<PersonAccount>, IValidator<Account>
    {
        public PersonAccountValidator()
        {
            RuleFor(a => a.FirstName).NotEmpty().WithMessage("Please Enter a first name");
            RuleFor(a => a.LastName).NotEmpty().WithMessage("Please enter a last name");
        }

        public FluentValidation.Results.ValidationResult Validate(Business.Models.Account instance)
        {
            return base.Validate((Business.Models.PersonAccount)instance);
        }
    }
}
