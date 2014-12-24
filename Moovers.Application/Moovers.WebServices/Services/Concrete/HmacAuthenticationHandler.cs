using System.Text;
using Business.Interfaces;
using Moovers.Webservices.Services;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Moovers.WebServices.Services.Concrete
{
    public class HmacAuthenticationHandler : DelegatingHandler
    {
        private const string Token = "session_token";

        private readonly ICustomAuthenticationRepository _secretRepository;

        private readonly IMessageRepresentationBuilder _representationBuilder;

        private readonly ISignatureCalculator _signatureCalculator;

        public HmacAuthenticationHandler(ICustomAuthenticationRepository secretRepository, IMessageRepresentationBuilder representationBuilder, ISignatureCalculator signatureCalculator)
          //  : base(innerHandler)
        {
            _secretRepository = secretRepository;
            _representationBuilder = representationBuilder;
            _signatureCalculator = signatureCalculator;
        }

        protected async Task<bool> IsAuthenticated(HttpRequestMessage requestMessage)
        {
            if (!_representationBuilder.IsRequestValid(requestMessage))
            {
                return false;
            }

            string username = requestMessage.Headers.GetValues(_representationBuilder.UsernameHeader).First();
            var secret = _secretRepository.GetSecretForUser(username);

            if (secret == null)
            {
                return false;
            }

            var representation = _representationBuilder.BuildRequestRepresentation(requestMessage);
            if (representation == null)
            {
                return false;
            }

            if (requestMessage.Content.Headers.ContentMD5 != null && !await IsMd5Valid(requestMessage))
            {
                return false;
            }

            var signature = _signatureCalculator.CalculateSignature(secret, representation);
            var result = (requestMessage.Headers.Authorization.Parameter == signature);
            return result;
        }

        protected async Task<bool> IsMd5Valid(HttpRequestMessage message)
        {
            return true;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            if(RouteValidator.ValidateLookupCall(requestMessage))
            {
                return await base.SendAsync(requestMessage,cancellationToken);
            }

            var isAuthenticated = await IsAuthenticated(requestMessage);

            StringBuilder sb = new StringBuilder();
            sb.Append(isAuthenticated.ToString()+"\n");
            sb.Append(requestMessage.Headers.GetValues("RequestDateTimeStamp").FirstOrDefault() + "--\n");
            sb.Append(requestMessage.Headers.GetValues("Authorization").FirstOrDefault() + "--\n");
            sb.Append(requestMessage.Headers.GetValues("ContentMD5").FirstOrDefault() + "--\n");
            sb.Append(requestMessage.Headers.GetValues("X-ApiAuth-Username") + "--\n");
            sb.Append(requestMessage.Content);
            sb.Append(requestMessage.ToString());
           
           string user = requestMessage.Headers.GetValues(_representationBuilder.UsernameHeader).First();

            _secretRepository.LogRequest(user,sb);

            if (!isAuthenticated)
            {
                string username = requestMessage.Headers.GetValues(_representationBuilder.UsernameHeader).First();
                var secret = _secretRepository.GetSecretForUser(username);
                var representation = _representationBuilder.BuildRequestRepresentation(requestMessage);
                var signature = _signatureCalculator.CalculateSignature(secret, representation);
                var response = requestMessage.CreateErrorResponse(HttpStatusCode.Unauthorized,"Not Authorized");
                return response;
            }

            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}