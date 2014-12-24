using Business.ViewModels;
using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class AddPaymentValidator : AbstractValidator<AddPaymentViewModel>
    {
        public AddPaymentValidator() 
        {
            RuleFor(m => m.amount).GreaterThan(0).When(m => m.method != Business.Utility.PaymentTypes.Other.ToString().ToLower()).WithMessage("Please enter a non-negative amount");

            RuleFor(m => m.amount).LessThanOrEqualTo(m => (m.GetTotalDue()*2)).When(m => m.quoteid != null && m.GetPaymentType() == Business.Utility.PaymentTypes.CreditCard).WithMessage("Amount Exceeds the required amount {0}", m =>m.GetTotalDue()*2);
            // non cashier check payments can only be accepted for >= 0 amounts
          
            RuleFor(m => m.amount).NotEqual(0).When(m => m.method == Business.Utility.PaymentTypes.Other.ToString()).WithMessage("Please enter an amount");

            // only quotes and work orders have payments
            RuleFor(m => m.quoteid).NotNull().When(i => !i.workOrderID.HasValue);
            RuleFor(m => m.workOrderID).NotNull().When(i => !i.quoteid.HasValue);

            // when paying w/ a stored Credit Card, a Guid is sent down of what to charge
            RuleFor(m => m.method).Must(m => Business.Utility.General.IsGuid(m))
                                  .When(m => m.method != "[-NEW CARD-]" && m.GetPaymentType() == Business.Utility.PaymentTypes.CreditCard)
                                  .WithMessage("Unable to process payment");

            // when paying w/ a new credit card, 
            RuleFor(m => m.cardnumber).CreditCard().When(i => i.IsNewCard()).WithMessage("Please enter a credit card number.");
        }
    }
}
