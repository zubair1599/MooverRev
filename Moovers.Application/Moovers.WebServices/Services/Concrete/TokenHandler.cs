using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Business.Interfaces;
using Moovers.Webservices.Services;

namespace Moovers.WebServices.Services.Concrete
{
    public class TokenHandler : DelegatingHandler
    {
    
        private readonly IMessageRepresentationBuilder _representationBuilder;

        private readonly ICustomAuthenticationRepository _secretRepository;
        public TokenHandler(ICustomAuthenticationRepository secretRepository, IMessageRepresentationBuilder representationBuilder)
        {
            _secretRepository = secretRepository;
            _representationBuilder = representationBuilder;
          
        }
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            if (RouteValidator.ValidateLookupCall(requestMessage) || RouteValidator.ValidateLoginCall(requestMessage))
            {
                return await base.SendAsync(requestMessage, cancellationToken);
            }

            string username = requestMessage.Headers.GetValues(_representationBuilder.UsernameHeader).First();
            string token = requestMessage.Headers.GetValues(_representationBuilder.SessionToken).First();

            if (!_secretRepository.ValidateToken(username, token))
            {              
                var response = requestMessage.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized");
                return response;
            }

            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}