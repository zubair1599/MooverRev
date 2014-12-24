using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Moovers.WebServices.Services.Concrete
{
    public class ResponseContentMd5Handler : DelegatingHandler
    {
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            if (RouteValidator.ValidateLookupCall(requestMessage))
            {
                return await base.SendAsync(requestMessage, cancellationToken);
            }

            HttpResponseMessage response = await base.SendAsync(requestMessage, cancellationToken);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var content = await response.Content.ReadAsByteArrayAsync();
                using (var md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(content);
                    response.Content.Headers.ContentMD5 = hash;
                }
            }

            return response;
        }
    }
}