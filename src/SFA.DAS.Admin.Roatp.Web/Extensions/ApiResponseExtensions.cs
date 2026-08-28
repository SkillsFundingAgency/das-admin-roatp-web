using System.Net;
using Refit;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class ApiResponseExtensions
{
    public static bool IsNotFoundOrBadRequest(this IApiResponse response)
        => response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest;
}
