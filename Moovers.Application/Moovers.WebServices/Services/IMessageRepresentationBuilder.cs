using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Moovers.Webservices.Services
{
    public interface IMessageRepresentationBuilder
    {
        string UsernameHeader { get; }

        string DateHeader { get; }

        string SessionToken { get; }

        string AuthenticationScheme { get; }

        string BuildRequestRepresentation(HttpRequestMessage requestMessage);

        bool IsRequestValid(HttpRequestMessage message);
    }
}