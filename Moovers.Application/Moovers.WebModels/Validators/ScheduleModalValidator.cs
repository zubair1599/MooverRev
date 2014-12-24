using Business.FirstData;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class ScheduleModalValidator : AbstractValidator<ScheduleModal>
    {
        public ScheduleModalValidator()
        {
            RuleFor(m => m.paymentType).NotEmpty();

            RuleFor(m => m.cardnumber).NotEmpty().CreditCard().When(m => m.paymentType == "NEW_CARD").WithMessage("Please enter a credit card number");
            RuleFor(m => m.name).NotEmpty().When(m => m.paymentType == "NEW_CARD").WithMessage("Please enter a billing name");
            ////RuleFor(m => m.cvv2).NotEmpty().When(m => m.paymentType == "NEW_CARD").WithMessage("Please enter a cvv2");
            ////RuleFor(m => m.billingzip).NotEmpty().When(m => m.paymentType == "NEW_CARD").WithMessage("Please enter a billing zip");
            RuleFor(m => m.expirationmonth).NotEmpty().When(m => m.paymentType == "NEW_CARD");
            RuleFor(m => m.expirationyear).NotEmpty().When(m => m.paymentType == "NEW_CARD");

            RuleFor(m => m.cardnumber).Must(Business.Utility.CreditCardPayment.IsValidCard).When(m => m.paymentType == "NEW_CARD").WithMessage("Please enter a credit card number");
            RuleFor(m => m.cardnumber).Must(m =>
                Business.Utility.CreditCardPayment.GetCardType(m) == FirstDataCardTypes.Discover ||
                Business.Utility.CreditCardPayment.GetCardType(m) == FirstDataCardTypes.Mastercard ||
                Business.Utility.CreditCardPayment.GetCardType(m) == FirstDataCardTypes.Visa ||
                Business.Utility.CreditCardPayment.GetCardType(m) == FirstDataCardTypes.Amex
            ).When(m => m.paymentType == "NEW_CARD").WithMessage("Invalid card type - {0}", m => Business.Utility.CreditCardPayment.GetCardType(m.cardnumber));
        }
    }
}
