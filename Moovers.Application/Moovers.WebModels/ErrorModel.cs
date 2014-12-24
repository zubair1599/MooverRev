using System.Collections.Generic;
using FluentValidation.Results;

namespace Moovers.WebModels
{
    public class ErrorModel
    {
        public IList<ValidationFailure> Errors { get; set; }

        public ErrorModel(ValidationResult errors)
        {
            this.Errors = errors.Errors;
        }

        public ErrorModel(string prop, string error)
        {
            var validationErrorr = new ValidationFailure(prop, error);
            var errors = new List<ValidationFailure> {
                validationErrorr
            };
            this.Errors = errors;
        }
    }
}