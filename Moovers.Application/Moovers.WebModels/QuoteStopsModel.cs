using System;
using System.Collections.Generic;
using System.Linq;
using Business.ToClean;
using Moovers.WebModels.Validators;
using Newtonsoft.Json;

namespace Moovers.WebModels
{
    public class QuoteStopsModel : QuoteEdit
    {
        public string stopsjson { get; set; }

        public IEnumerable<StopJson> GetStops()
        {
            if (String.IsNullOrEmpty(this.stopsjson))
            {
                return Enumerable.Empty<StopJson>();
            }

            return JsonConvert.DeserializeObject<IEnumerable<StopJson>>(this.stopsjson);
        }

        public FluentValidation.Results.ValidationResult Validate()
        {
            var validator = new StopsValidator();
            return validator.Validate(this);
        }

        public QuoteStopsModel(Business.Models.Quote quote)
            : base(quote, "Stops")
        {
            this.Quote = quote;
        }
    }
}