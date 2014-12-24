using Business.Models;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class EmailAddressValidator : AbstractValidator<EmailAddress>
    {
        public EmailAddressValidator()
        {
            RuleFor(e => e.Email).NotEmpty().EmailAddress().WithMessage("Please enter a valid e-mail address");
        }
    }
}
