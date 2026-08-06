using SFA.DAS.Admin.Roatp.Domain.Models;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class UnrestrictedCoursesService(IOuterApiClient outerApiClient, ISessionService sessionService) : IUnrestrictedCoursesService
{
    public async Task<List<RestrictedCourseModel>> GetUnrestrictedCourses(CancellationToken cancellationToken)
    {
        var savedCourses = sessionService.Get<List<RestrictedCourseModel>>(SessionKeys.GetUnrestrictedCourses);
        if (savedCourses != null) return savedCourses;

        var response = await outerApiClient.GetRestrictedCourses(restricted: false, cancellationToken);
        savedCourses = response.Courses;
        sessionService.Set(SessionKeys.GetUnrestrictedCourses, savedCourses);

        return savedCourses;
    }
}
