using System;
using Business.ViewModels;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class AccountModelValidator<T> : AbstractValidator<AccountModelBase<T>>
        where T : Business.Models.Account
    {
        public AccountModelValidator()
        {
            RuleFor(m => m.BillingAddress).NotNull().SetValidator(new AddressValidator());
            RuleFor(m => m.MailingAddress).NotNull().SetValidator(new AddressValidator());
            RuleFor(m => m.PrimaryPhone).NotNull().SetValidator(new PhoneNumberValidator());

            When(m => !String.IsNullOrEmpty(m.PrimaryEmail.Email), () => RuleFor(m => m.PrimaryEmail).SetValidator(new EmailAddressValidator()));
            When(m => !String.IsNullOrEmpty(m.SecondaryEmail.Email), () => RuleFor(m => m.SecondaryEmail).SetValidator(new EmailAddressValidator()));
            When(m => !String.IsNullOrEmpty(m.SecondaryPhone.Number), () => RuleFor(m => m.SecondaryPhone).SetValidator(new PhoneNumberValidator()));
            When(m => !String.IsNullOrEmpty(m.FaxPhone.Number), () => RuleFor(m => m.FaxPhone).SetValidator(new PhoneNumberValidator()));

            if (typeof(T) == typeof(Business.Models.PersonAccount))
            {
                RuleFor(m => m.Account).SetValidator(new PersonAccountValidator());
            }

            if (typeof(T) == typeof(Business.Models.BusinessAccount))
            {
                RuleFor(m => m.Account).SetValidator(new BusinessAccountValidator());
            }
        }
    }
}
