using SFA.DAS.Admin.Roatp.Domain.Interfaces;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Web.Infrastructure;

public class RestrictedCoursesApiClient(IOuterApiClient outerApiClient) : IRestrictedCoursesApiClient
{
    public Task<GetRestrictedCoursesResponse> GetRestrictedCourses(
        bool restricted,
        CancellationToken cancellationToken)
    {
        return outerApiClient.GetRestrictedCourses(restricted, cancellationToken);
    }
}
