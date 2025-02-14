using System.Net.Http.Headers;
using Deepin.Domain;

namespace Deepin.Infrastructure;

public class HttpClientAuthorizationDelegatingHandler(IUserContext userContext) : DelegatingHandler
{
    private readonly IUserContext _userContext = userContext;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Headers.Authorization == null)
        {
            var accessToken = _userContext.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}