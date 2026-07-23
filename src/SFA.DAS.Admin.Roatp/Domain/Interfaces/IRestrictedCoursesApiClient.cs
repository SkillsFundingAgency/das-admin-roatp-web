using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Domain.Interfaces;

public interface IRestrictedCoursesApiClient
{
    Task<GetRestrictedCoursesResponse> GetRestrictedCourses(
        bool restricted,
        CancellationToken cancellationToken);
}
