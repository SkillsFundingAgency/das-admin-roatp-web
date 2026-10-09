using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;
using SFA.DAS.Admin.Roatp.Web.Infrastructure;

namespace SFA.DAS.Admin.Roatp.Web.Services;

public class CoursesService(
    IOuterApiClient outerApiClient,
    IApplicationCacheService applicationCacheService) : ICoursesService
{
    public async Task<GetCourseResponse?> GetCourse(string larsCode, CancellationToken cancellationToken)
    {
        var courses = await GetCourses(cancellationToken);
        return courses.FirstOrDefault(course => course.LarsCode == larsCode);
    }

    private async Task<List<GetCourseResponse>> GetCourses(CancellationToken cancellationToken)
    {
        var cachedCourses = await applicationCacheService.GetAsync<GetCoursesResponse>(
            ApplicationCacheKeys.CoursesCacheKey, cancellationToken);
        if (cachedCourses is not null)
        {
            return cachedCourses.Courses;
        }

        var response = await outerApiClient.GetCourses(cancellationToken);

        await response.EnsureSuccessStatusCodeAsync();

        await applicationCacheService.SetAsync(ApplicationCacheKeys.CoursesCacheKey, response.Content, cancellationToken: cancellationToken);

        return response.Content?.Courses ?? [];
    }
}
