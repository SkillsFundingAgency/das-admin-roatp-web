using System.Net;
using Refit;

namespace SFA.DAS.Admin.Roatp.Web.Extensions;

public static class ApiResponseStatusExtensions
{
    public static bool IsNotFound(this IApiResponse response)
        => response.StatusCode == HttpStatusCode.NotFound;
}
