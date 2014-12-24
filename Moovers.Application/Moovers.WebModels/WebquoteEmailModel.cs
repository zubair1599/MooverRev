using Business.ViewModels;

namespace Moovers.WebModels
{
    public class WebquoteEmailModel : EmailModel
    {
        public Business.Models.Quote Quote { get; set; }
    }
}
