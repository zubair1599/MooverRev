using FluentValidation;

namespace Moovers.WebModels.Validators
{
    public class UserAccountValidator : AbstractValidator<AccountEditModel>
    {
        public UserAccountValidator()
        {
            RuleFor(m => m.FirstName).NotEmpty();
            RuleFor(m => m.LastName).NotEmpty();
            RuleFor(m => m.User.UserName).NotEmpty();
            RuleFor(m => m.Password).Equal(m => m.ConfirmPassword);
            RuleFor(m => m.Email).EmailAddress();
            RuleFor(m => m.Phone).NotEmpty().Length(10);
            RuleFor(m => m.Title).NotEmpty();
            RuleFor(m => m.FranchiseIds).NotEmpty();
            RuleFor(m => m.FranchiseIds).NotNull();
        }
    }
}
