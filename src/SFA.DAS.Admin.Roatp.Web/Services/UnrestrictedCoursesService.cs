using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class UnrestrictedCoursesService(IOuterApiClient outerApiClient) : IUnrestrictedCoursesService
{
    public async Task<List<RestrictedCourseModel>> GetUnrestrictedCourses(CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetRestrictedCourses(restricted: false, cancellationToken);
        return response.Courses;
    }
}
