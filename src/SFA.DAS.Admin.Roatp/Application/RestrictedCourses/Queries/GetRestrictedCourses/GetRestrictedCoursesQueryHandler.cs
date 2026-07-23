using MediatR;
using SFA.DAS.Admin.Roatp.Domain.Interfaces;
using SFA.DAS.Admin.Roatp.Domain.OuterApi.Responses;

namespace SFA.DAS.Admin.Roatp.Application.RestrictedCourses.Queries.GetRestrictedCourses;

public class GetRestrictedCoursesQueryHandler(IRestrictedCoursesApiClient outerApiClient)
    : IRequestHandler<GetRestrictedCoursesQuery, GetRestrictedCoursesQueryResult>
{
    public async Task<GetRestrictedCoursesQueryResult> Handle(GetRestrictedCoursesQuery request, CancellationToken cancellationToken)
    {
        var response = await outerApiClient.GetRestrictedCourses(
            request.Restricted,
            cancellationToken);

        var filteredResult = ApplyFilters(request, response);

        return new GetRestrictedCoursesQueryResult
        {
            TotalCount = filteredResult.TotalCount,
            Courses = filteredResult.Courses
        };
    }

    private static GetRestrictedCoursesResponse ApplyFilters(GetRestrictedCoursesQuery request, GetRestrictedCoursesResponse response)
    {
        var courses = response.Courses.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.CourseName))
        {
            var searchTerm = request.CourseName.Trim();
            courses = courses.Where(c =>
                c.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                || c.LarsCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        if (request.LearningTypes.Count > 0 && request.LearningTypes.Count < 3)
        {
            courses = courses.Where(c => request.LearningTypes.Contains(c.LearningType));
        }

        var filteredCourses = courses.ToList();

        return new GetRestrictedCoursesResponse
        {
            Courses = filteredCourses,
            TotalCount = filteredCourses.Count
        };
    }
}
