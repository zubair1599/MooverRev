using System.Linq;
using Business.Models;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class AddressValidator : AbstractValidator<Address>
    {
        public AddressValidator()
        {
            //RuleFor(a => a.Street1).NotEmpty();
            RuleFor(a => a.City).NotEmpty();
            RuleFor(a => a.State).NotEmpty().Must(m => Business.Utility.General.StateDictionary.Select(s => s.Value).Contains(m));
            RuleFor(a => a.Zip).NotEmpty();
        }
    }
}
