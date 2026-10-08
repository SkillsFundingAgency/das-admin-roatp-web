using System.Net;
using Refit;

namespace SFA.DAS.Admin.Roatp.Web.UnitTests.TestHelpers;

public static class OuterApiResponse
{
    public static ApiResponse<T> Create<T>(
        HttpStatusCode statusCode,
        T? content = default,
        HttpMethod? method = null,
        bool includeException = true)
    {
        var httpResponse = new HttpResponseMessage(statusCode);
        ApiException? apiException = null;
        if (includeException && !httpResponse.IsSuccessStatusCode)
        {
            apiException = ApiException.Create(
                new HttpRequestMessage(),
                method ?? HttpMethod.Get,
                httpResponse,
                new RefitSettings()).GetAwaiter().GetResult();
        }

        return new ApiResponse<T>(httpResponse, content!, new RefitSettings(), apiException);
    }
}
