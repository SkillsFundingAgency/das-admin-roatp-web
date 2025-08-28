using System.Net.Http.Headers;
using SFA.DAS.Api.Common.Interfaces;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public class InnerApiAuthenticationHeaderHandler(IAzureClientCredentialHelper _azureClientCredentialHelper, string _apiIdentifier) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Version", "1.0");
        if (!string.IsNullOrEmpty(_apiIdentifier))
        {
            var accessToken = await _azureClientCredentialHelper.GetAccessTokenAsync(_apiIdentifier);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}